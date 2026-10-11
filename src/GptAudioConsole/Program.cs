
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

internal static class Program
{
    private const string Model = "gpt-realtime-2";
    private const int SampleRate = 24000;

    private static ClientWebSocket? _ws;
    private static WaveInEvent? _microphone;
    private static WaveOutEvent? _speaker;
    private static BufferedWaveProvider? _playbackBuffer;

    // ClientWebSocket permits only one concurrent send.
    private static readonly SemaphoreSlim SendLock = new(1, 1);

    private static async Task Main()
    {
        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Set the OPENAI_API_KEY environment variable first.");
            return;
        }

        using var cancellation = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };

        _ws = new ClientWebSocket();
        _ws.Options.SetRequestHeader(
            "Authorization", "Bearer " + apiKey);

        _playbackBuffer = new BufferedWaveProvider(
            new WaveFormat(SampleRate, 16, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(10),
            DiscardOnBufferOverflow = true
        };

        _speaker = new WaveOutEvent();
        _speaker.Init(_playbackBuffer);
        _speaker.Play();

        try
        {
            var uri = new Uri($"wss://api.openai.com/v1/realtime?model={Model}");

            Console.WriteLine($"Connecting to {Model}...");

            await _ws.ConnectAsync(uri, cancellation.Token);

            Console.WriteLine("Connected.");

            // Configure a bidirectional speech session.
            await SendEventAsync(new
            {
                type = "session.update",
                session = new
                {
                    type = "realtime",
                    model = Model,
                    instructions =
                        "You are a friendly voice assistant. " +
                        "Answer naturally and concisely. " +
                        "Speak clearly and conversationally.",
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
                            turn_detection = new
                            {
                                type = "semantic_vad"
                            }
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

            StartMicrophone();

            Console.WriteLine();
            Console.WriteLine("Speak into your microphone.");
            Console.WriteLine("Press Ctrl+C to exit.");
            Console.WriteLine();

            await ReceiveLoopAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when Ctrl+C is pressed.
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
            _microphone?.StopRecording();
            _microphone?.Dispose();

            if (_ws?.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Application closing",
                        CancellationToken.None);
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

    private static void StartMicrophone()
    {
        _microphone = new WaveInEvent
        {
            WaveFormat = new WaveFormat(SampleRate, 16, 1),
            BufferMilliseconds = 100,
            NumberOfBuffers = 3
        };

        _microphone.DataAvailable += (_, e) =>
        {
            if (_ws?.State != WebSocketState.Open ||
                e.BytesRecorded <= 0)
                return;

            // Copy the bytes because NAudio reuses its event buffer.
            byte[] audio = new byte[e.BytesRecorded];
            Buffer.BlockCopy(
                e.Buffer, 0, audio, 0, e.BytesRecorded);

            // Do not block the audio capture callback on network I/O.
            _ = SendAudioSafelyAsync(audio);
        };

        _microphone.RecordingStopped += (_, e) =>
        {
            if (e.Exception != null)
                Console.WriteLine(
                    $"Microphone error: {e.Exception.Message}");
        };

        _microphone.StartRecording();
    }

    private static async Task SendAudioSafelyAsync(byte[] audio)
    {
        try
        {
            await SendEventAsync(new
            {
                type = "input_audio_buffer.append",
                audio = Convert.ToBase64String(audio)
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (_ws?.State == WebSocketState.Open)
                Console.WriteLine(
                    $"Audio send error: {ex.Message}");
        }
    }

    private static async Task SendEventAsync(
        object value, CancellationToken cancellationToken)
    {
        if (_ws == null ||
            _ws.State != WebSocketState.Open)
            return;

        string json = JsonSerializer.Serialize(value);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await SendLock.WaitAsync(cancellationToken);
        try
        {
            if (_ws.State == WebSocketState.Open)
            {
                await _ws.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);
            }
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

        while (!cancellationToken.IsCancellationRequested &&
               _ws.State == WebSocketState.Open)
        {
            using var message = new System.IO.MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await _ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType ==
                    WebSocketMessageType.Close)
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
                Console.WriteLine("Voice session configured.");
                break;

            case "input_audio_buffer.speech_started":
                Console.WriteLine();
                Console.WriteLine("You are speaking...");
                break;

            case "input_audio_buffer.speech_stopped":
                Console.WriteLine("Processing your speech...");
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

                    try
                    {
                        _playbackBuffer?.AddSamples(
                            audio, 0, audio.Length);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine(
                            "Audio playback buffer is full.");
                    }
                }
                break;

            case "response.output_audio_transcript.done":
                Console.WriteLine();
                break;

            case "response.done":
                Console.WriteLine();
                Console.WriteLine("--- Response complete ---");
                break;

            case "error":
                Console.WriteLine(
                    "API error: " + root.GetRawText());
                break;
        }
    }
}
