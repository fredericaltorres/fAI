using DynamicSugar;
using S = System;
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
using System.Text.Json.Serialization;

namespace fAI.SourceCodeAnalysis
{
    public class ExceptionAnalyzer : JsonObject
    {
        public string Language { get; set; } = "C#";
        public string Case { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }

        private List<FileLocation> _stackTraceInfo { get; set; }  = new List<FileLocation>();
        
        public List<FileLocation> StackTraceInfo {
            get
            {
                if(!string.IsNullOrEmpty(StackTrace))
                    return ExtractFileInformationFromStackTrace(StackTrace);

                return _stackTraceInfo;
            }
            set             {
                _stackTraceInfo = value;
            }
        }

        public List<string> OtherFiles = new List<string>();
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string Context { get; set; } = "You are a helpful and experienced C# and .NET software developer.";


        const string EndOfMessageInException = @" at ";

        public static ExceptionAnalyzer ExtractFromLog(string text)
        {
            //var regExFindSystemDotException = new Regex(@"System\.([0..9a..zA..Z_\.]*)Exception:", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var regExFindSystemDotException = new Regex(@"System\.(.*?)Exception:", RegexOptions.IgnoreCase | RegexOptions.Singleline); // UnGreedy
            
            var match = regExFindSystemDotException.Match(text);
            if (match.Success && match.Captures.Count == 1)
            {
                var exceptionName = match.Captures[0].Value; // Extract the exception name
                exceptionName = exceptionName.Substring(0, exceptionName.Length - 1); // Remove the trailing colon

                var messageIndex = match.Index + match.Length; // Extract the message 
                var messageEndIndex = text.IndexOf(EndOfMessageInException, messageIndex);
                var message = text.Substring(messageIndex, messageEndIndex - messageIndex);
                message = message.Trim(); // Clean up the message
                var nextIndex = messageEndIndex;

                var fileLocations = new List<FileLocation>();
                var nextText = text.Substring(nextIndex); // Extract the stack trace information
                var regExS = new Regex(@"\sat\s+(?<method>.+?)\s+in\s+(?<file>.+?):line\s+(?<line>\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var matches = regExS.Matches(nextText);
                foreach (Match matchS in matches)
                {
                    if (matchS.Success)
                    {
                        var method = matchS.Groups["method"].Value.Trim();
                        var file = matchS.Groups["file"].Value.Trim();
                        var line = matchS.Groups["line"].Value;
                        fileLocations.Add(new FileLocation
                        {
                            MethodName = method,
                            FileName = file,
                            LineNumber = int.Parse(line)
                        });
                        fileLocations.Last().Clean();
                        var localFileName = fileLocations.Last().GetLocalFileName();
                        var localFileFound = fileLocations.Last().LocalFileFound;
                    }
                }

                var ea = new ExceptionAnalyzer { 
                    ExceptionType = exceptionName,
                    Message = message,
                    StackTraceInfo = fileLocations,

                };
                return ea;
            }
            return null;
        }

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

        [JsonIgnore]
        public string OtherFilesSourceCodeWithLineNumbers
        {
            get
            {
                if (OtherFiles.Count == 0) return null;
                var sb = new StringBuilder();
                sb.AppendLine("Other files:");
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

        [JsonIgnore]
        public string FunctionName
        {
            get
            {
                if (string.IsNullOrEmpty(TargetSite))
                    return TargetSite;
                var tokens = new Tokenizer().Tokenize(TargetSite);
                if (tokens.Count > 1)
                    return $"{tokens[1].Value}()";
                return TargetSite;
            }
        }

        [JsonIgnore]
        public string SourceCodeWithLineNumbers
        {
            get
            {
                var fileInfo = StackTraceInfo[0];
                if (File.Exists(fileInfo.FileName))
                    return PrepareSourceCodeFileForAnalysis(fileInfo.FileName);

                if (fileInfo.LocalFileFound)
                    return PrepareSourceCodeFileForAnalysis(fileInfo.GetLocalFileName());

                return "[Source Code Not Found]";
            }
        }

        public static string GetJsonFileName(string Case)
        {
            return Path.Combine(GetCaseRootFolder(), $"{Case}.ExceptionAnalysis.json");
        }

        public static ExceptionAnalyzer Load(string @case)
        {
            if (File.Exists(@case))
                return FromFile<ExceptionAnalyzer>(@case);

            return FromFile<ExceptionAnalyzer>(GetJsonFileName(@case));
        }

        public ExceptionAnalyzer()
        {
        }

        public ExceptionAnalyzer(Exception ex, string @case, List<string> otherFiles)
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

        public const string RootFolder = @"c:\temp\fAI.RunTimeAnalysis";

        public static string GetCaseRootFolder()
        {
            var dir = RootFolder;
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
            return $"{MDCodeBlock}{GetLanguageCodeFromExtension(ext)}\n{numberedCode}\n{MDCodeBlock}";
        }

        public string PromptAnalyzeCodeProposeNewFunction
        {
            get
            {
                var promptStr = $@"
Analyze the following {Language}, fileName ""{SourceCodeFileNameOnly}"", for the following Exception: ""{ExceptionType}""
at line {SourceCodeLine}. Answer in MARKDOWN syntax.

Propose a new version of the function ""{FunctionName}"" to fix the issue.
Source Code File ""{SourceCodeFileNameOnly}"":
{SourceCodeWithLineNumbers}
";
                return promptStr.Trim();

            }
        }

        // Answer in MARKDOWN syntax.

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

{OtherFilesSourceCodeWithLineNumbers}
";
                return promptStr.Trim();
            }
        }

        public static string RunCode(Action actionCode, List<string> otherFiles = null, [CallerMemberName] string callerCaseName = null)
        {
            try
            {
                actionCode();
                return null;
            }
            catch (Exception ex)
            {
                var ea = new ExceptionAnalyzer(ex, callerCaseName, otherFiles);
                return ea.JsonFileName;
            }
        }

        public string AnalyzeAndGenerateAnalysisReport(Anthropic_Prompt_Claude_3_5_Sonnet p)
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

            var client = new FAI();
            var response = client.Completions.Create(prompt);
            var analysisReportFileName = this.GenerateAnalysisReport(this.JsonFileName, prompt, response);
            return analysisReportFileName;
        }

        public string AnalyzeAndGenerateReport(Prompt_GPT_4o p)
        {
            var client = new FAI();
            var prompt = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = this.Context },
                    new GPTMessage { Role =  MessageRole.user, Content = this.PromptAnalyzeCodeProposeExplanation }
                },
            };
            var response = client.Completions.Create(prompt);
            var analysisReportFileName = this.GenerateAnalysisReport(this.JsonFileName, prompt, response);
            return analysisReportFileName;
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




