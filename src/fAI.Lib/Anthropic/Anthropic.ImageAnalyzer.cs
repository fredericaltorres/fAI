using fAI;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AnthropicImageAnalysis
{
    public class ImageAnalyzer
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        //private const string Model = "claude-opus-4-7";
        private const string AnthropicVersion = "2023-06-01";

        private string _apiKey;

        public ImageAnalyzer(string apiKey = null)
        {
            if (apiKey == null)
                apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            _apiKey = apiKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Analyzes an image from a file path and returns a detailed description.
        /// </summary>
        public (string analysis, string title, GenericAICompletions.GenericAIUsage usage) AnalyzeImageFromFile(string model, string imagePath, string prompt = @"
Please analyze this image thoroughly and provide:
1. **Overall Description** - A concise summary of what the image shows.
2. **Key Elements** - List the main subjects, objects, or focal points.
3. **Colors & Composition** - Describe the dominant colors, lighting, and layout.
4. **Context & Setting** - Infer the environment, time of day, or scenario if possible.
5. **Mood & Atmosphere** - Describe the emotional tone or feeling conveyed.
6. **Notable Details** - Highlight any interesting, unusual, or fine-grained details.
Be specific and descriptive in your analysis.

Use MARKDOWN syntax for formatting the response, with headings and bullet points where appropriate.
",
            string language = "english"
            )
        {
            var usage = new GenericAICompletions.GenericAIUsage(model, prompt, "");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!File.Exists(imagePath))
                    throw new FileNotFoundException($"Image file not found: {imagePath}");

                var imageBytes = File.ReadAllBytes(imagePath);
                var mediaType = GetMediaType(imagePath);
                var (analysis, analyzeImageUsage) = AnalyzeImage(model, imageBytes, mediaType, prompt);
                usage.Add(analyzeImageUsage);

                // Use the same model for the title generation to keep usage consistent
                var genericAI = new GenericAI(ApiKey: _apiKey);
                var titleResponse = genericAI.Completions.GenerateTitle(analysis, language: language, model: model);
                var title = titleResponse.Title;
                var marker = "# Title";
                if (title.StartsWith(marker))
                {
                    title = title.Substring(marker.Length).Trim();
                }
                usage.Add(titleResponse.Usage);

                var finalTitle = titleResponse.Title.Replace("*", "").Replace("\n", "").Replace("\r", "");
                return (analysis, finalTitle, usage);
            }
            finally
            { 
                sw.Stop();
                usage.SetDuration(sw);
                HttpBase.Trace($"[AnalyzeImage] model: {model}, duration: {sw.ElapsedMilliseconds/1000.0:0.000} s, {imagePath}", this); 
            }
        }

        public (string text, GenericAICompletions.GenericAIUsage usage) AnalyzeImage(string model, byte[] imageBytes, string mediaType, string prompt, int maxTokens = 16 * 1024)
        {
            var usage = new GenericAICompletions.GenericAIUsage(model, prompt, ""); 
            string base64Image = Convert.ToBase64String(imageBytes);
            var requestBody = new
            {
                model = model,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64", media_type = mediaType, data = base64Image
                                }
                            },
                            new
                            {
                                type = "text", text = prompt
                            }
                        }
                    }
                }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            HttpBase.Trace($"Body: {jsonBody}", this);
            HttpBase.Trace($"Url: {ApiUrl}", this);

            using (var content = new StringContent(jsonBody, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = _httpClient.PostAsync(ApiUrl, content).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"API request failed [{response.StatusCode}]: {responseBody}");

                var r =  ParseResponse(responseBody);
                HttpBase.Trace($"Response: {responseBody}", this);
                return (r, usage);
            }
        }

        private static string ParseResponse(string responseJson)
        {
            using (var doc = JsonDocument.Parse(responseJson))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("content", out JsonElement contentArray) && contentArray.GetArrayLength() > 0)
                {
                    JsonElement firstBlock = contentArray[0];
                    if (firstBlock.TryGetProperty("text", out JsonElement textElement))
                        return textElement.GetString() ?? "No description returned.";
                }

                return "Unable to parse the model response.";
            }
        }

        /// <summary>
        /// Determines the MIME type from the file extension.
        /// </summary>
        private static string GetMediaType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            switch(ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    throw new NotSupportedException($"Unsupported image format '{ext}'. Supported: jpg, jpeg, png, gif, webp.");
            };
        }
    }

    //// -------------------------------------------------------------------------
    //// Example usage
    //// -------------------------------------------------------------------------
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        string apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? throw new InvalidOperationException("ANTHROPIC_API_KEY environment variable is not set.");

    //        ImageAnalyzer analyzer = new ImageAnalyzer(apiKey);

    //        // --- Analyze from file path ---
    //        string imagePath = "sample.jpg"; // replace with your image path
    //        Console.WriteLine("Analyzing image...\n");

    //        string description = analyzer.AnalyzeImageFromFile(imagePath);
    //        Console.WriteLine("=== Image Analysis ===");
    //        Console.WriteLine(description);

    //        // --- Or supply a custom prompt ---
    //        string customDescription = analyzer.AnalyzeImageFromFile(
    //            imagePath,
    //            customPrompt: "What objects are in this image? List them as bullet points.");
    //        Console.WriteLine("\n=== Custom Prompt Analysis ===");
    //        Console.WriteLine(customDescription);
    //    }
    //}
}