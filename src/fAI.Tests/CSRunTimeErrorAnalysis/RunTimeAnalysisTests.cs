using ChatGPT.Tests.CSRunTimeErrorAnalysis;
using DynamicSugar;
using fAI;
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
using Xunit;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;


/*
 
using c# and the openai completion api https://api.openai.com/v1/chat/completions
how to pass a file to be analyzed for runtime errors?

 */
namespace fAI.Tests
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

    public class ExceptionAnalyzed : DynamicSugar.JsonObject
    {
        public string Language { get; set; } = "C#";
        public string Case { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public List<FileLocation> StackTraceInfo => ExtractFileInformationFromStackTrace(StackTrace);

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

        public List<string> OtherFiles = new List<string>();

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
                          .AppendLine($"File: {Path.GetFileName(file)}")
                          .AppendLine(PrepareSourceCodeFileForAnalysis(file)).AppendLine();
                    else
                        sb.AppendLine($"File: {Path.GetFileName(file)} - [Source Code Not Found]");
                }
                return sb.ToString();
            }
        }   

        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string FunctionName
        {
            get
            {
                var tokens = new DynamicSugar.Tokenizer().Tokenize(TargetSite);
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
                return ExceptionAnalyzed.FromFile<ExceptionAnalyzed>(@case);

            return ExceptionAnalyzed.FromFile<ExceptionAnalyzed>(GetJsonFileName(@case));
        }

        public ExceptionAnalyzed()
        {

        }

        public ExceptionAnalyzed(Exception ex, string @case)
        {
            this.Case = @case;
            this.Message = ex.Message;
            this.ExceptionType = ex.GetType().FullName;
            this.StackTrace = ex.StackTrace;
            this.Source = ex.Source;
            this.TargetSite = ex.TargetSite?.ToString();
            this.Case = @case;
            base.JsonFileName = GetJsonFileName(@case);
            base.Save();
        }

        public static string GetCaseRootFolder()
        {
            var dir = Path.Combine(@"c:\temp", "fAI.RunTimeAnalysis");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        static List<FileLocation> ExtractFileInformationFromStackTrace(string stackTrace)
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

        public static string PrepareSourceCodeFileForAnalysis(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            var numberedCode = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
                numberedCode.AppendLine($"{i + 1,4}: {lines[i]}");
            return numberedCode.ToString();
        }

        public string Prompt
        {
            get
            {

                var promptStr = $@"
Analyze the following {this.Language}, fileName ""{this.SourceCodeFileNameOnly}"", for the following Exception: ""{this.ExceptionType}""
at line {this.SourceCodeLine}.

Propose a new version of the function ""{this.FunctionName}"" to fix the issue.
Source Code File ""{this.SourceCodeFileNameOnly}"":
{this.SourceCodeWithLineNumbers}
";
                return promptStr;

            }
        }

        public string Prompt2
        {
            get
            {

                var promptStr = $@"
Analyze the following {this.Language}, fileName ""{this.SourceCodeFileNameOnly}"", for the following Exception: ""{this.ExceptionType}""
at line {this.SourceCodeLine}.

Propose an explanation.
Source Code File ""{this.SourceCodeFileNameOnly}"":
{this.SourceCodeWithLineNumbers}


Other files:
{this.OtherFilesSourceCodeWithLineNumbers}


";
                return promptStr;

            }
        }

        public string Context => "You are a helpful and experienced C# and .NET software developer.";

        public static string RunCase(System.Action a, [CallerMemberName] string callerCaseName = null)
        {
            try
            {
                a();
                return null;
            }
            catch (Exception ex)
            {
                var ea = new ExceptionAnalyzed(ex, callerCaseName);
                return ea.JsonFileName;
            }
        }

    }









    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class RunTimeAnalysisTests : OpenAIUnitTestsBase
    {
        public RunTimeAnalysisTests()
        {
            OpenAI.TraceOn = true;
        }


        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitialization()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCase(new System.Action(() =>
            {
                var runTimeCase = new RunTimeAnalysis_Case1();
                var result = runTimeCase.Run(1); // This will throw a DivideByZeroException
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);

            var client = new OpenAI();
            var prompt = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = ea.Context },
                    new GPTMessage { Role =  MessageRole.user, Content = ea.Prompt }
                },
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            var t = response.Text;
        }


        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitializationInConfigFile()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCase(new System.Action(() =>
            {
                var runTimeCase = new RunTimeAnalysis_Case2();
                var result = runTimeCase.Run(1); // This will throw a DivideByZeroException
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);

            ea.OtherFiles.Add( @"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\app.config");

            var client = new OpenAI();
            var prompt = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = ea.Context },
                    new GPTMessage { Role =  MessageRole.user, Content = ea.Prompt2 }
                },
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            var t = response.Text;
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitializationInConfigFile_v2()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCase(new System.Action(() =>
            {
                var runTimeCase = new RunTimeAnalysis_Case3();
                var result = runTimeCase.Run(1); // This will throw a DivideByZeroException
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);

            ea.OtherFiles.Add(@"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\app.config");

            var client = new OpenAI();
            var prompt = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = ea.Context },
                    new GPTMessage { Role =  MessageRole.user, Content = ea.Prompt2 }
                },
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            var t = response.Text;
        }
    }
}



