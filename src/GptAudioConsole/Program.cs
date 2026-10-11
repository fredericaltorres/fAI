
using Deepgram.Models;
using fAI;
using fAI.OpenAIModel.ImageResponseGpt;
using NAudio.Wave;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal static class Program
{

    //Model Audio input Audio output Text input Text output
    //gpt-realtime-2.1	$32.00	$64.00	$4.00	$24.00
    //gpt-realtime-2.1-mini	$10.00	$20.00	$0.60	$2.40
    private const string Model = "gpt-realtime-2.1";
    //private const string Model = "gpt-realtime-2.1-mini";
    
    private const int SampleRate = 24000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;

    private static ClientWebSocket? _ws;
    private static WaveOutEvent? _speaker;
    private static BufferedWaveProvider? _playbackBuffer;

    private static readonly SemaphoreSlim SendLock = new(1, 1);
    private static TaskCompletionSource<bool> NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static TaskCompletionSource<bool> _sessionUpdated = NewSignal();
    private static TaskCompletionSource<bool> _responseDone = NewSignal();

    public static string TraceError(string message, [CallerMemberName] string methodName = "")
    {
        return Trace("[ERROR]"+message, true, methodName);
    }
    public static string Trace(string message, bool toConsole = true, [CallerMemberName] string methodName = "")
    {
        HttpBase.Trace(message, null, methodName);
        if (toConsole)
        {
            Console.WriteLine(message);
        }
        return message; 
    }

    private static async Task Main(string[] args)
    {
        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Trace("Set the OPENAI_API_KEY environment variable first.");
            return;
        }

        string systemPromptOrSkill = File.ReadAllText(@"C:\DVT\winspeak\WinSpeakApp\WinSpeakApp\Skills\genealogist\SKILL.md");
        var contextData = File.ReadAllText(@"C:\Users\FredericTorres\Dropbox\MARKDOWN\PERSONAL\Family Tree - Frederic Torres.md");

        using var cancellation = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            Trace($"Freddy Audio/AI");
            Trace($"Connecting to {Model}...");

            _ws = new ClientWebSocket();
            _ws.Options.SetRequestHeader("Authorization", "Bearer " + apiKey);
            var uri = new Uri($"wss://api.openai.com/v1/realtime?model={Model}");

            await _ws.ConnectAsync(uri, cancellation.Token);

            // Start receiving server events before configuring the session.
            Task receiveTask = ReceiveLoopAsync(cancellation.Token);

            InitAudioOut();

            await SendUserText(contextData, cancellation);
            await SendUserAudioConfig(systemPromptOrSkill, cancellation);
            await _sessionUpdated.Task.WaitAsync(cancellation.Token);

            Console.WriteLine();
            Console.WriteLine("Ready.");
            Console.WriteLine("Press R to start recording.");
            Console.WriteLine("Press R again to stop and send.");
            Console.WriteLine("Press Esc to exit.");
            Console.WriteLine();

            bool isRecording = false;
            WaveInEvent? microphone = null;
            MemoryStream? recording = null;
            TaskCompletionSource<bool>? recordingStopped = null;
            object recordingLock = new();

            while (!cancellation.IsCancellationRequested)
            {
                //ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;

                if (key.Key == ConsoleKey.R)
                {
                    if (!isRecording)
                    {
                        // START A NEW LOCAL RECORDING.
                        recording = new MemoryStream();
                        recordingStopped = NewSignal();
                        microphone = new WaveInEvent
                        {
                            WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                            BufferMilliseconds = 100,
                            NumberOfBuffers = 3
                        };

                        MemoryStream activeRecording = recording;
                        object activeLock = recordingLock;

                        microphone.DataAvailable += (_, e) =>
                        {
                            lock (activeLock) { activeRecording.Write(e.Buffer, 0, e.BytesRecorded); }
                        };

                        TaskCompletionSource<bool> stoppedSignal = recordingStopped;

                        microphone.RecordingStopped += (_, e) =>
                        {
                            if (e.Exception != null)
                                Console.WriteLine($"Microphone error: {e.Exception.Message}");
                            stoppedSignal.TrySetResult(true);
                        };

                        microphone.StartRecording();
                        isRecording = true;
                        Console.WriteLine("Recording... Press R to stop.");
                    }
                    else
                    {
                        // STOP RECORDING BEFORE SENDING ANYTHING TO OPENAI.
                        isRecording = false;
                        microphone!.StopRecording();
                        await recordingStopped!.Task.WaitAsync(cancellation.Token);
                        microphone.Dispose();
                        microphone = null;
                        byte[] audio;
                        lock (recordingLock)
                        {
                            audio = recording!.ToArray();
                            recording.Dispose();
                            recording = null;
                        }
                        Trace($"Recorded {audio.Length:N0} bytes.");
                        if (audio.Length == 0)
                        {
                            Trace("No audio was recorded.");
                            continue;
                        }

                        Trace("Sending audio to OpenAI...");
                        // Prevent an empty or stale input buffer from being included in this turn.
                        await SendEventAsync(new { type = "input_audio_buffer.clear" }, cancellation.Token);
                        const int chunkSize = 24 * 1024; // Send the completed recording in manageable chunks.

                        for (int offset = 0; offset < audio.Length; offset += chunkSize)
                        {
                            int count = Math.Min(chunkSize, audio.Length - offset);
                            byte[] chunk = new byte[count];
                            Buffer.BlockCopy(audio, offset, chunk, 0, count);
                            await SendEventAsync(new { type = "input_audio_buffer.append", audio = Convert.ToBase64String(chunk) }, cancellation.Token);
                        }

                        // Commit the complete audio recording as one turn.
                        await SendEventAsync(new { type = "input_audio_buffer.commit" }, cancellation.Token);
                        _responseDone = NewSignal();
                        // Request a spoken response.
                        await SendEventAsync(new { type = "response.create", response = new { output_modalities = new[] { "audio" } } }, cancellation.Token);
                        Trace("Waiting for the response...");
                        await _responseDone.Task.WaitAsync(cancellation.Token);
                        Trace("");
                        Trace("Response complete. Press R to speak again.");
                    }
                }


            } // Main While Loop 

            if (isRecording && microphone != null)
            {
                microphone.StopRecording();
                if (recordingStopped != null)
                {
                    try
                    {
                        await recordingStopped.Task.WaitAsync(TimeSpan.FromSeconds(2));
                    }
                    catch (TimeoutException)
                    {
                        // Continue shutdown if the device doesn't stop.
                    }
                }
                microphone.Dispose();
            }
            recording?.Dispose();
            cancellation.Cancel();
            try
            {
                await receiveTask;
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled.");
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
        finally
        {
            if (_ws?.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,"Application closing", CancellationToken.None);
                }
                catch
                {
                    // Ignore errors during shutdown.
                }
            }

            _ws?.Dispose();
            _speaker?.Stop();
            _speaker?.Dispose();
            SendLock.Dispose();

            Console.WriteLine("Disconnected.");
        }
    }

    private static async Task SendUserAudioConfig(string systemPromptOrSkill, CancellationTokenSource cancellation)
    {
        // Configure manual push-to-talk input.
        await SendEventAsync(new
        {
            type = "session.update",
            session = new
            {
                type = "realtime",
                model = Model,
                instructions = systemPromptOrSkill,
                output_modalities = new[] { "audio" },
                audio = new
                {
                    input = new
                    {
                        format = new { type = "audio/pcm", rate = SampleRate },
                        turn_detection = (object?)null // We explicitly commit audio after recording.
                    },
                    output = new
                    {
                        format = new { type = "audio/pcm", rate = SampleRate },
                        voice = "marin"
                    }
                }
            }
        }, cancellation.Token);
    }

    private static async Task SendUserText(string contextData, CancellationTokenSource cancellation)
    {
        await SendEventAsync(new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[] { new { type = "input_text", text = contextData } }
            }
        }, cancellation.Token);
    }

    private static void InitAudioOut()
    {
        _playbackBuffer = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
        {
            BufferDuration = TimeSpan.FromSeconds(30),
            DiscardOnBufferOverflow = false
        };

        _speaker = new WaveOutEvent();
        _speaker.Init(_playbackBuffer);
        _speaker.Play();
    }

    private static async Task SendEventAsync(object value, CancellationToken cancellationToken)
    {
        if (_ws == null || _ws.State != WebSocketState.Open)
        {
            throw new WebSocketException("The WebSocket is not connected.");
        }
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        await SendLock.WaitAsync(cancellationToken);
        try
        {
            if (_ws.State != WebSocketState.Open)
                throw new WebSocketException("The WebSocket connection was closed.");
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            SendLock.Release();
        }
    }

    private static async Task ReceiveLoopAsync( CancellationToken cancellationToken)
    {
        if (_ws == null)
            return;

        byte[] buffer = new byte[64 * 1024];

        while (!cancellationToken.IsCancellationRequested && _ws.State == WebSocketState.Open)
        {
            using var message = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    return;
                message.Write(buffer, 0, result.Count);

            } while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text)
                continue;

            ProcessServerEvent(Encoding.UTF8.GetString(message.ToArray()));
        }
    }

    private static void ProcessServerEvent(string json)
    {
        if (json.Contains(@"""response.output_audio.delta""") || json.Contains(@"""response.output_audio.delta""") || json.Contains(@"""response.output_audio_transcript.delta"""))
        {
            // do nothing
        }
        else 
        { 
            Trace($"Json: {json}", toConsole: false);
        }
        
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("type", out JsonElement typeElement))
            return;

        string type = typeElement.GetString() ?? "";

        switch (type)
        {
            case "session.created":
                Trace("Realtime session created.");
                break;

            case "session.updated":
                _sessionUpdated.TrySetResult(true);
                break;

            case "input_audio_buffer.committed":
                break;

            case "response.output_audio_transcript.delta":
                if (root.TryGetProperty("delta", out JsonElement transcriptDelta))
                {
                    Console.Write(transcriptDelta.GetString());
                }
                break;

            case "response.output_audio.delta":
                if (root.TryGetProperty("delta", out JsonElement audioDelta))
                {
                    byte[] audio = Convert.FromBase64String(audioDelta.GetString() ?? "");
                    _playbackBuffer?.AddSamples(audio, 0, audio.Length);
                }
                break;

            case "response.output_audio_transcript.done":
                break;

            case "response.done":
                Trace("Response done.");
                _responseDone.TrySetResult(true);
                break;

            case "error":
                Trace("OpenAI API error: " + root.GetRawText());
                _sessionUpdated.TrySetException(new InvalidOperationException(root.GetRawText()));
                _responseDone.TrySetResult(true);
                break;
        }
    }
}
