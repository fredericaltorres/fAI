using DynamicSugar;
using S = System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;


/*
 
using c# and the openai completion api https://api.openai.com/v1/chat/completions
how to pass a file to be analyzed for runtime errors?

 */
namespace fAI.SourceCodeAnalysis
{
    public class FileLocation
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public override string ToString()
        {
            return $"{FileName}:{LineNumber}";
        }
    }

    public class ExceptionAnalyzed : JsonObject
    {
        public string Language { get; set; } = "C#";
        public string Case { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public List<FileLocation> StackTraceInfo => ExtractFileInformationFromStackTrace(StackTrace);
        public List<string> OtherFiles = new List<string>();
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string Context { get; set; } = "You are a helpful and experienced C# and .NET software developer.";

        public int SourceCodeLine
        {
            get
            {
                if (StackTraceInfo.Count > 0)
                    return StackTraceInfo[0].LineNumber;
                return -1;
            }
        }

        public string SourceCodeFileNameOnly
        {
            get
            {
                if (StackTraceInfo.Count > 0)
                    return Path.GetFileName(StackTraceInfo[0].FileName);
                return null;
            }
        }

        public string SourceCodeFileName
        {
            get
            {
                if (StackTraceInfo.Count > 0)
                    return StackTraceInfo[0].FileName;
                return null;
            }
        }

        public string OtherFilesSourceCodeWithLineNumbers
        {
            get
            {
                if (OtherFiles.Count == 0) return null;
                var sb = new StringBuilder();
                foreach (var file in OtherFiles)
                {
                    if (File.Exists(file))
                        sb.AppendLine()
                          .AppendLine($@"File: ""{Path.GetFileName(file)}""")
                          .AppendLine(PrepareSourceCodeFileForAnalysis(file)).AppendLine();
                    else
                        sb.AppendLine($"File: {Path.GetFileName(file)} - [Source Code Not Found]");
                }
                return sb.ToString();
            }
        }   
        
        public string FunctionName
        {
            get
            {
                var tokens = new Tokenizer().Tokenize(TargetSite);
                if (tokens.Count > 1)
                    return $"{tokens[1].Value}()";
                return TargetSite;
            }
        }

        public string SourceCodeWithLineNumbers
        {
            get
            {
                var fileInfo = StackTraceInfo[0];
                if (File.Exists(fileInfo.FileName))
                    return PrepareSourceCodeFileForAnalysis(fileInfo.FileName);
                return "[Source Code Not Found]";
            }
        }

        private static string GetJsonFileName(string Case)
        {
            return Path.Combine(GetCaseRootFolder(), $"{Case}.error.json");
        }

        public static ExceptionAnalyzed Load(string @case)
        {
            if (File.Exists(@case))
                return FromFile<ExceptionAnalyzed>(@case);

            return FromFile<ExceptionAnalyzed>(GetJsonFileName(@case));
        }

        public ExceptionAnalyzed()
        {
        }

        public ExceptionAnalyzed(Exception ex, string @case, List<string> otherFiles)
        {
            Case = @case;
            Message = ex.Message;
            ExceptionType = ex.GetType().FullName;
            StackTrace = ex.StackTrace;
            Source = ex.Source;
            TargetSite = ex.TargetSite?.ToString();
            Case = @case;
            JsonFileName = GetJsonFileName(@case);
            OtherFiles = otherFiles ?? new List<string>();
            Save();
        }

        public static string GetCaseRootFolder()
        {
            var dir = Path.Combine(@"c:\temp", "fAI.RunTimeAnalysis");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        private static List<FileLocation> ExtractFileInformationFromStackTrace(string stackTrace)
        {
            var fileInformation = new List<FileLocation>();
            if (string.IsNullOrEmpty(stackTrace))
                return fileInformation;
            var lines = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(@"in\s+(?<file>.+?):line\s+(?<line>\d+)", RegexOptions.IgnoreCase);
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var fileName = match.Groups["file"].Value;
                    var lineNumber = int.Parse(match.Groups["line"].Value);
                    fileInformation.Add(new FileLocation { FileName = fileName, LineNumber = lineNumber });
                }
            }
            return fileInformation;
        }

        private static string GetLanguageCodeFromExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            switch (ext)
            {
                case ".cs": return "csharp";
                case ".xml": return "xml";
                case ".config": return "xml";

                default: return "Unknown";
            }
        }

        const string MDCodeBlock = "```";

        public static string PrepareSourceCodeFileForAnalysis(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            string[] lines = File.ReadAllLines(fileName);

            var numberedCode = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
                numberedCode.AppendLine($"{i + 1,4}: {lines[i]}");
            return $"{MDCodeBlock}\n{GetLanguageCodeFromExtension(ext)}\n{numberedCode}\n{MDCodeBlock}";
        }

        public string PromptAnalyzeCodeProposeNewFunction
        {
            get
            {
                var promptStr = $@"
Analyze the following {Language}, fileName ""{SourceCodeFileNameOnly}"", for the following Exception: ""{ExceptionType}""
at line {SourceCodeLine}.

Propose a new version of the function ""{FunctionName}"" to fix the issue.
Source Code File ""{SourceCodeFileNameOnly}"":
{SourceCodeWithLineNumbers}
";
                return promptStr;

            }
        }

        public string PromptAnalyzeCodeProposeExplanation
        {
            get
            {
                var promptStr = $@"
Analyze the following {Language}, fileName ""{SourceCodeFileNameOnly}"", for the following Exception: ""{ExceptionType}""
at line {SourceCodeLine}.

Propose an explanation.
Source Code File ""{SourceCodeFileNameOnly}"":
{SourceCodeWithLineNumbers}

Other files:
{OtherFilesSourceCodeWithLineNumbers}
";
                return promptStr;
            }
        }

        public static string RunCase(Action actionCode, List<string> otherFiles = null, [CallerMemberName] string callerCaseName = null)
        {
            try
            {
                actionCode();
                return null;
            }
            catch (Exception ex)
            {
                var ea = new ExceptionAnalyzed(ex, callerCaseName, otherFiles);
                return ea.JsonFileName;
            }
        }

        public string GenerateAnalysisReport(string jsonFileName, GPTPrompt prompt, CompletionResponse completionResponse)
        {
            var reportFileName = Path.ChangeExtension(jsonFileName, ".report.md");
            var sb = new StringBuilder();
            sb.AppendLine($"## Prompt({prompt.Model}):");
            sb.AppendLine($"{prompt.FullPrompt}");
            sb.AppendLine($"## Answer:");
            sb.AppendLine($"{completionResponse.Text}");

            File.WriteAllText(reportFileName, sb.ToString());
            return reportFileName;
        }

        public string GenerateAnalysisReport(string jsonFileName, AnthropicPromptBase prompt, CompletionResponse completionResponse)
        {
            var reportFileName = Path.ChangeExtension(jsonFileName, ".report.md");
            var sb = new StringBuilder();
            sb.AppendLine($"## Prompt({prompt.Model}):");
            sb.AppendLine($"{prompt.FullPrompt}");
            sb.AppendLine($"## Answer:");
            sb.AppendLine($"{completionResponse.Text}");

            File.WriteAllText(reportFileName, sb.ToString());
            return reportFileName;
        }
    }
}




