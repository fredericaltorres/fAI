using DynamicSugar;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace fAI.SourceCodeAnalysis
{
    public class CodeAnalysis : JsonObject
    {
        public string Language { get; set; } = "C#";
        public string Case { get; set; }
        public FileLocation FileLocation { get; set; }
        public string Context { get; set; } = "You are a helpful and experienced C# and .NET software developer.";

        [JsonIgnore]
        public string SourceCodeFileNameOnly => Path.GetFileName(FileLocation.FileName);
        [JsonIgnore]
        public int SourceCodeLine => FileLocation.LineNumber;
        [JsonIgnore]
        public string SourceCodeWithLineNumbers => ExceptionAnalyzer.PrepareSourceCodeFileForAnalysis(FileLocation.FileName);
        [JsonIgnore]
        public string MethodName => FileLocation.MethodName;
        [JsonIgnore]
        public string ClassName => FileLocation.ClassName;

        public static CodeAnalysis Load(string @case)
        {
            if (File.Exists(@case))
                return FromFile<CodeAnalysis>(@case);

            return FromFile<CodeAnalysis>(GetJsonFileName(@case));
        }

        private static string GetJsonFileName(string Case)
        {
            return Path.Combine(ExceptionAnalyzer.GetCaseRootFolder(), $"{Case}.CodeAnalysis.json");
        }

        public string GenerateAnalysisReport(string jsonFileName, AnthropicPromptBase prompt, fAI.AnthropicLib.AnthropicCompletionResponse completionResponse)
        {
            var reportFileName = Path.ChangeExtension(jsonFileName, ".report.md");
            var sb = new StringBuilder();
            sb.AppendLine($"## Prompt({prompt.Model}):");
            sb.AppendLine($"{prompt.FullPrompt}");
            sb.AppendLine($"## Text:");
            sb.AppendLine($"{completionResponse.Text}");

            File.WriteAllText(reportFileName, sb.ToString());
            return reportFileName;
        }

        public string AnalyzeCodeAndGenerateReport()
        {
            var prompt = new Anthropic_Prompt_Claude_3_5_Sonnet()
            {
                System = this.Context,
                Messages = DS.List(
                       new AnthropicMessage(MessageRole.user, DS.List<AnthropicContentMessage>(
                           new AnthropicContentText(this.PromptAnalyzeCodeProposeExplanation))
                       )
                   )
            };

            var client = new Anthropic();
            var response = client.Completions.Create(prompt);

            var analysisReportFileName = this.GenerateAnalysisReport(this.JsonFileName, prompt, response);
            return analysisReportFileName;
        }

        public CodeAnalysis()
        {

        }

        public CodeAnalysis(string @case, FileLocation fileLocation)
        {
            Case = @case;
            FileLocation = fileLocation;
            JsonFileName = GetJsonFileName(@case);
            Save();
        }

        public string PromptAnalyzeCodeProposeExplanation
        {
            get
            {
                var promptStr = $@"
The {Language}, method ""{this.MethodName}"", in class ""{this.ClassName}""
line {SourceCodeLine}, does not return the expected value OR behave as expected. Text in MARKDOWN syntax.

Propose an explanation.
Source Code File ""{SourceCodeFileNameOnly}"":
{SourceCodeWithLineNumbers}
";
                return promptStr.Trim();
            }
        }
    }
}

