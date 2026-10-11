
using NAudio.Wave;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal static class Program
{
    private const string Model = "gpt-realtime-2";
    private const int SampleRate = 24000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;

    private static ClientWebSocket? _ws;
    private static WaveOutEvent? _speaker;
    private static BufferedWaveProvider? _playbackBuffer;

    private static readonly SemaphoreSlim SendLock = new(1, 1);

    private static TaskCompletionSource<bool> _sessionUpdated =
        NewSignal();

    private static TaskCompletionSource<bool> _responseDone =
        NewSignal();

    private static TaskCompletionSource<bool> NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task Main(string[] args)
    {
        string? apiKey =
            Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine(
                "Set the OPENAI_API_KEY environment variable first.");
            return;
        }

        string systemPrompt = File.ReadAllText(@"C:\DVT\winspeak\WinSpeakApp\WinSpeakApp\Skills\genealogist\SKILL.md");
        var contextData = File.ReadAllText(@"C:\Users\FredericTorres\Dropbox\MARKDOWN\PERSONAL\Family Tree - Frederic Torres.md");

        using var cancellation = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            _ws = new ClientWebSocket();
            _ws.Options.SetRequestHeader("Authorization", "Bearer " + apiKey);

            var uri = new Uri($"wss://api.openai.com/v1/realtime?model={Model}");

            Console.WriteLine($"Connecting to {Model}...");

            await _ws.ConnectAsync(uri, cancellation.Token);

            // Start receiving server events before configuring the session.
            Task receiveTask = ReceiveLoopAsync(cancellation.Token);

            _playbackBuffer = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
            {
                BufferDuration = TimeSpan.FromSeconds(30),
                DiscardOnBufferOverflow = false
            };

            _speaker = new WaveOutEvent();
            _speaker.Init(_playbackBuffer);
            _speaker.Play();

            await SendEventAsync(new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[]
                    {
                            new
                            {
                                type = "input_text", text = contextData
                            }
                        }
                    }
            }, cancellation.Token);

            // Configure manual push-to-talk input.
            await SendEventAsync(new
            {
                type = "session.update",
                session = new
                {
                    type = "realtime",
                    model = Model,
                    instructions = systemPrompt,
                    output_modalities = new[] { "audio" },
                    audio = new
                    {
                        input = new
                        {
                            format = new
                            {
                                type = "audio/pcm",
                                rate = SampleRate
                            },
                            // We explicitly commit audio after recording.
                            turn_detection = (object?)null
                        },
                        output = new
                        {
                            format = new
                            {
                                type = "audio/pcm",
                                rate = SampleRate
                            },
                            voice = "marin"
                        }
                    }
                }
            }, cancellation.Token);

            await _sessionUpdated.Task.WaitAsync(
                cancellation.Token);

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
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                    break;

                if (key.Key != ConsoleKey.R)
                    continue;

                if (!isRecording)
                {
                    // Start a new local recording.
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
                        lock (activeLock)
                        {
                            activeRecording.Write(e.Buffer, 0, e.BytesRecorded);
                        }
                    };

                    TaskCompletionSource<bool> stoppedSignal = recordingStopped;

                    microphone.RecordingStopped += (_, e) =>
                    {
                        if (e.Exception != null)
                        {
                            Console.WriteLine($"Microphone error: {e.Exception.Message}");
                        }

                        stoppedSignal.TrySetResult(true);
                    };

                    microphone.StartRecording();
                    isRecording = true;

                    Console.WriteLine("Recording... Press R to stop.");
                }
                else
                {
                    // Stop recording before sending anything to OpenAI.
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
                    Console.WriteLine($"Recorded {audio.Length:N0} bytes.");
                    if (audio.Length == 0)
                    {
                        Console.WriteLine("No audio was recorded.");
                        continue;
                    }

                    Console.WriteLine("Sending audio to OpenAI...");
                    // Prevent an empty or stale input buffer from
                    // being included in this turn.
                    await SendEventAsync(new
                    {
                        type = "input_audio_buffer.clear"
                    }, cancellation.Token);

                    // Send the completed recording in manageable chunks.
                    const int chunkSize = 24 * 1024;

                    for (int offset = 0; offset < audio.Length;offset += chunkSize)
                    {
                        int count = Math.Min(chunkSize, audio.Length - offset);

                        byte[] chunk = new byte[count];
                        Buffer.BlockCopy(audio, offset, chunk, 0, count);

                        await SendEventAsync(new
                        {
                            type = "input_audio_buffer.append",
                            audio = Convert.ToBase64String(chunk)
                        }, cancellation.Token);
                    }

                    // Commit the complete audio recording as one turn.
                    await SendEventAsync(new
                    {
                        type = "input_audio_buffer.commit"
                    }, cancellation.Token);

                    _responseDone = NewSignal();

                    // Request a spoken response.
                    await SendEventAsync(new
                    {
                        type = "response.create",
                        response = new
                        {
                            output_modalities = new[] { "audio" }
                        }
                    }, cancellation.Token);

                    Console.WriteLine("Waiting for the response...");

                    await _responseDone.Task.WaitAsync(cancellation.Token);

                    Console.WriteLine();
                    Console.WriteLine("Response complete. Press R to speak again.");
                }
            }

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

    private static async Task ReceiveLoopAsync(
        CancellationToken cancellationToken)
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

            ProcessServerEvent(
                Encoding.UTF8.GetString(message.ToArray()));
        }
    }

    private static void ProcessServerEvent(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("type", out JsonElement typeElement))
            return;

        string type = typeElement.GetString() ?? "";

        switch (type)
        {
            case "session.created":
                Console.WriteLine("Realtime session created.");
                break;

            case "session.updated":
                Console.WriteLine("Session configured.");
                _sessionUpdated.TrySetResult(true);
                break;

            case "input_audio_buffer.committed":
                Console.WriteLine("Audio input committed.");
                break;

            case "response.output_audio_transcript.delta":
                if (root.TryGetProperty(
                    "delta", out JsonElement transcriptDelta))
                {
                    Console.Write(transcriptDelta.GetString());
                }
                break;

            case "response.output_audio.delta":
                if (root.TryGetProperty(
                    "delta", out JsonElement audioDelta))
                {
                    byte[] audio = Convert.FromBase64String(
                        audioDelta.GetString() ?? "");

                    _playbackBuffer?.AddSamples(
                        audio, 0, audio.Length);
                }
                break;

            case "response.output_audio_transcript.done":
                Console.WriteLine();
                break;

            case "response.done":
                Console.WriteLine();
                _responseDone.TrySetResult(true);
                break;

            case "error":
                Console.WriteLine(
                    "OpenAI API error: " + root.GetRawText());

                _sessionUpdated.TrySetException(
                    new InvalidOperationException(root.GetRawText()));

                _responseDone.TrySetResult(true);
                break;
        }
    }
}
