using NAudio.Wave;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AudioStreamingDemo;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("OPENAI_API_KEY not found");
            return;
        }

        await StreamAudioAndText(apiKey);
    }

    private static async Task StreamAudioAndText(string apiKey)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var body = new
        {
            model = "gpt-audio-1.5",
            stream = true,
            input = "Tell me a short story about a dragon and a knight.",
            modalities = new[] { "text", "audio" },
            audio = new
            {
                voice = "alloy",
                format = "pcm16"
            }
        };

        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var requesth = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        requesth.Content = content;

        using var response = await httpClient.SendAsync( requesth, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        //--------------------------------------
        // NAudio playback setup
        //--------------------------------------

        var waveFormat = new WaveFormat(
            rate: 24000,
            bits: 16,
            channels: 1);

        var bufferedProvider =
            new BufferedWaveProvider(waveFormat)
            {
                DiscardOnBufferOverflow = true
            };

        using var waveOut = new WaveOutEvent();

        waveOut.Init(bufferedProvider);
        waveOut.Play();

        Console.WriteLine("Streaming...\n");

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith("data:"))
                continue;

            var payload = line.Substring(5).Trim();

            if (payload == "[DONE]")
                break;

            try
            {
                using var doc = JsonDocument.Parse(payload);

                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeProperty))
                    continue;

                var eventType = typeProperty.GetString();

                //-------------------------------------------------
                // TEXT DELTA
                //-------------------------------------------------
                if (eventType == "response.output_text.delta")
                {
                    var delta =
                        root.GetProperty("delta").GetString();

                    Console.Write(delta);
                }

                //-------------------------------------------------
                // AUDIO DELTA
                //-------------------------------------------------
                else if (eventType == "response.audio.delta")
                {
                    var audioBase64 =
                        root.GetProperty("delta").GetString();

                    if (!string.IsNullOrEmpty(audioBase64))
                    {
                        byte[] pcmBytes =
                            Convert.FromBase64String(audioBase64);

                        bufferedProvider.AddSamples(
                            pcmBytes,
                            0,
                            pcmBytes.Length);
                    }
                }

                //-------------------------------------------------
                // ERROR
                //-------------------------------------------------
                else if (eventType == "error")
                {
                    Console.WriteLine();
                    Console.WriteLine("API Error:");
                    Console.WriteLine(payload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Parse Error: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Finished");

        while (bufferedProvider.BufferedBytes > 0)
        {
            await Task.Delay(100);
        }
    }
}