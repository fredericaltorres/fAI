using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NAudio.Utils;
using NAudio.Wave;

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
const string Model = "gpt-audio-1.5";
const string ChatEndpoint = "https://api.openai.com/v1/chat/completions";
const string Voice = "alloy";          // alloy, ash, ballad, coral, echo, sage, shimmer, verse, marin, cedar
const int OutputSampleRate = 24000;    // pcm16 output from the API is 24 kHz, 16-bit, mono
const int RecordSampleRate = 16000;    // microphone capture: 16 kHz, 16-bit, mono (ideal for speech)
const int MaxRecordSeconds = 60;       // safety cap on recording length / upload size
const double MinRecordSeconds = 0.3;   // ignore accidental taps
const int MaxInputLength = 4000;       // basic validation for typed input
const int MaxHistoryMessages = 20;     // keep the context bounded
const int MaxAudioTurnsInHistory = 3;  // audio input tokens are expensive: only keep the latest voice turns

// The API key is read from the environment and never hardcoded or printed.
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Please set the OPENAI_API_KEY environment variable.");
    return 1;
}

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

// ---------------------------------------------------------------------------
// NAudio playback setup: a buffered provider we push PCM chunks into
// ---------------------------------------------------------------------------
var outputFormat = new WaveFormat(OutputSampleRate, 16, 1);
var audioBuffer = new BufferedWaveProvider(outputFormat)
{
    BufferDuration = TimeSpan.FromMinutes(5),
    DiscardOnBufferOverflow = true
};

using var waveOut = new WaveOutEvent { DesiredLatency = 200 };
waveOut.Init(audioBuffer);
waveOut.Play();

// Ctrl+C cancels the current recording / answer instead of killing the app.
CancellationTokenSource? currentRequest = null;
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    currentRequest?.Cancel();
    audioBuffer.ClearBuffer();
};

var history = new List<JsonObject>
{
    new()
    {
        ["role"] = "system",
        ["content"] = "You are a friendly, concise voice assistant. " +
                      "The user may speak to you through audio messages; respond naturally to what they say."
    }
};

Console.WriteLine($"Chatting with {Model}. Ctrl+C stops the current action.\n");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("press [R] to record, [T] to type, [Q] to quit");
    Console.ResetColor();

    var key = Console.ReadKey(intercept: true).Key;
    Console.WriteLine();

    if (key is ConsoleKey.Q or ConsoleKey.Escape)
        break;
    if (key is not (ConsoleKey.R or ConsoleKey.T))
        continue;

    currentRequest = new CancellationTokenSource();
    try
    {
        JsonObject userMessage;

        if (key == ConsoleKey.R)
        {
            var wav = await RecordAudioAsync(currentRequest.Token);
            if (wav is null) continue;

            // Send the recording directly to the model as an input_audio content part.
            userMessage = new JsonObject
            {
                ["role"] = "user",
                ["content"] = new JsonArray(
                    new JsonObject
                    {
                        ["type"] = "input_audio",
                        ["input_audio"] = new JsonObject
                        {
                            ["data"] = Convert.ToBase64String(wav),
                            ["format"] = "wav"
                        }
                    })
            };
        }
        else
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;
            if (input.Length > MaxInputLength)
            {
                Console.WriteLine($"Input too long (max {MaxInputLength} characters).\n");
                continue;
            }
            userMessage = new JsonObject { ["role"] = "user", ["content"] = input };
        }

        history.Add(userMessage);
        TrimHistory(history, MaxHistoryMessages, MaxAudioTurnsInHistory);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Assistant: ");
        Console.ResetColor();

        var transcript = await StreamResponseAsync(http, history, audioBuffer, currentRequest.Token);
        Console.WriteLine("\n");

        if (transcript.Length > 0)
        {
            // Store the transcript as text so the conversation keeps its context.
            history.Add(new JsonObject { ["role"] = "assistant", ["content"] = transcript });
        }
        else
        {
            // No answer: drop the user turn so a failed request doesn't pollute the history.
            history.Remove(userMessage);
        }

        // Wait until the audio has finished playing before prompting/recording again,
        // so the microphone doesn't pick up the assistant's voice.
        while (audioBuffer.BufferedBytes > 0 && !currentRequest.IsCancellationRequested)
            await Task.Delay(100);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\n[cancelled]\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n[network error] {ex.Message}\n");
    }
    catch (NAudio.MmException ex)
    {
        Console.WriteLine($"\n[audio device error] {ex.Message}\n");
    }
    finally
    {
        currentRequest.Dispose();
        currentRequest = null;
    }
}

waveOut.Stop();
return 0;

// ---------------------------------------------------------------------------
// Records from the default microphone until R / Enter is pressed again
// (or the max duration is reached). Returns a WAV file as a byte array.
// ---------------------------------------------------------------------------
static async Task<byte[]?> RecordAudioAsync(CancellationToken ct)
{
    if (WaveInEvent.DeviceCount == 0)
    {
        Console.WriteLine("[no microphone found]\n");
        return null;
    }

    var format = new WaveFormat(RecordSampleRate, 16, 1);
    long maxBytes = (long)format.AverageBytesPerSecond * MaxRecordSeconds;

    using var memory = new MemoryStream();
    var writer = new WaveFileWriter(new IgnoreDisposeStream(memory), format);
    var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    long recordedBytes = 0;

    using var waveIn = new WaveInEvent { WaveFormat = format, BufferMilliseconds = 50 };
    waveIn.DataAvailable += (_, e) =>
    {
        if (recordedBytes >= maxBytes) return;
        int count = (int)Math.Min(e.BytesRecorded, maxBytes - recordedBytes);
        writer.Write(e.Buffer, 0, count);
        Interlocked.Add(ref recordedBytes, count);
        if (recordedBytes >= maxBytes) waveIn.StopRecording();
    };
    waveIn.RecordingStopped += (_, e) =>
    {
        writer.Dispose(); // finalizes the WAV header; underlying MemoryStream stays open
        if (e.Exception is not null) stopped.TrySetException(e.Exception);
        else stopped.TrySetResult();
    };

    waveIn.StartRecording();

    Console.ForegroundColor = ConsoleColor.Red;
    while (!stopped.Task.IsCompleted)
    {
        double seconds = (double)Interlocked.Read(ref recordedBytes) / format.AverageBytesPerSecond;
        Console.Write($"\r● Recording {seconds,4:F1}s  (press R or Enter to stop) ");

        if (ct.IsCancellationRequested)
        {
            waveIn.StopRecording();
            break;
        }
        if (Console.KeyAvailable)
        {
            var k = Console.ReadKey(intercept: true).Key;
            if (k is ConsoleKey.R or ConsoleKey.Enter)
            {
                waveIn.StopRecording();
                break;
            }
        }
        await Task.Delay(50);
    }
    Console.ResetColor();
    Console.WriteLine();

    await stopped.Task;
    ct.ThrowIfCancellationRequested();

    if ((double)recordedBytes / format.AverageBytesPerSecond < MinRecordSeconds)
    {
        Console.WriteLine("[recording too short]\n");
        return null;
    }

    return memory.ToArray();
}

// ---------------------------------------------------------------------------
// Sends the chat request and processes the Server-Sent Events stream.
// Audio chunks -> NAudio buffer, transcript chunks -> console.
// ---------------------------------------------------------------------------
static async Task<string> StreamResponseAsync(
    HttpClient http,
    List<JsonObject> history,
    BufferedWaveProvider audioBuffer,
    CancellationToken ct)
{
    var messages = new JsonArray();
    foreach (var m in history)
        messages.Add(m.DeepClone());

    var body = new JsonObject
    {
        ["model"] = Model,
        ["modalities"] = new JsonArray("text", "audio"),
        ["audio"] = new JsonObject { ["voice"] = Voice, ["format"] = "pcm16" },
        ["stream"] = true,
        ["messages"] = messages
    };

    using var request = new HttpRequestMessage(HttpMethod.Post, ChatEndpoint)
    {
        Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
    };

    using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync(ct);
        Console.WriteLine($"\n[API error {(int)response.StatusCode}] {ExtractErrorMessage(error)}");
        return string.Empty;
    }

    var transcript = new StringBuilder();

    await using var stream = await response.Content.ReadAsStreamAsync(ct);
    using var reader = new StreamReader(stream, Encoding.UTF8);

    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync(ct);
        if (string.IsNullOrEmpty(line) || !line.StartsWith("data:", StringComparison.Ordinal))
            continue;

        var payload = line["data:".Length..].Trim();
        if (payload == "[DONE]") break;

        JsonDocument doc;
        try { doc = JsonDocument.Parse(payload); }
        catch (JsonException) { continue; } // ignore malformed chunks

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                continue;
            if (!choices[0].TryGetProperty("delta", out var delta))
                continue;

            // Plain text content (used if the model replies without audio)
            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
            {
                var text = content.GetString();
                Console.Write(text);
                transcript.Append(text);
            }

            if (delta.TryGetProperty("audio", out var audio))
            {
                // Streamed transcript of the spoken audio -> console
                if (audio.TryGetProperty("transcript", out var t) && t.ValueKind == JsonValueKind.String)
                {
                    var text = t.GetString();
                    Console.Write(text);
                    transcript.Append(text);
                }

                // Streamed base64 PCM16 audio -> NAudio
                if (audio.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.String)
                {
                    try
                    {
                        var pcm = Convert.FromBase64String(data.GetString()!);
                        audioBuffer.AddSamples(pcm, 0, pcm.Length);
                    }
                    catch (FormatException)
                    {
                        // skip invalid audio chunk
                    }
                }
            }
        }
    }

    return transcript.ToString();
}

static string ExtractErrorMessage(string json)
{
    try
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("error", out var err) &&
            err.TryGetProperty("message", out var msg))
            return msg.GetString() ?? "Unknown error";
    }
    catch (JsonException) { }
    return "Unknown error";
}

static void TrimHistory(List<JsonObject> history, int maxMessages, int maxAudioTurns)
{
    // Keep the system message (index 0) and the most recent messages.
    while (history.Count > maxMessages + 1)
        history.RemoveAt(1);

    // Audio input is billed on every request it is resent in, so only the most recent
    // voice turns keep their audio. Older ones are replaced with a short placeholder;
    // the assistant's replies (stored as text) still carry the context of the conversation.
    int audioTurnsSeen = 0;
    for (int i = history.Count - 1; i >= 1; i--)
    {
        if (history[i]["content"] is JsonArray parts &&
            parts.Any(p => p?["type"]?.GetValue<string>() == "input_audio"))
        {
            audioTurnsSeen++;
            if (audioTurnsSeen > maxAudioTurns)
                history[i]["content"] = "[earlier voice message]";
        }
    }
}