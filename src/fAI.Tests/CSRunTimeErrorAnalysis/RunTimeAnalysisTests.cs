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
        public string Case { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public List<FileLocation> StackTraceInfo => ExtractFileInformationFromStackTrace(StackTrace);
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string SourceCodeWithLineNumbers 
        { 
            get {
                var fileInfo = StackTraceInfo[0];
                if (File.Exists(fileInfo.FileName))
                    return PrepareSourceCodeFileForAnalysis(fileInfo.FileName);
                return "[Source Code Not Found]";
            }   
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
            base.JsonFileName = Path.Combine(GetCaseRootFolder(), $"{@case}.error.json");
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
    }

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class RunTimeAnalysisTests : OpenAIUnitTestsBase
    {
        public RunTimeAnalysisTests()
        {
            OpenAI.TraceOn = true;
        }

   
        public string RunCase(System.Action a, 
            [CallerMemberName] string callerCaseName = null
        {
            var errorFileName = Path.Combine(GetCaseRootFolder(), $"{callerCaseName}.error.txt");
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

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_Case1_RunError()
        {
            var exceptionDescriptionFileName = RunCase(new System.Action(() =>
            {
                var runTimeCase = new RunTimeAnalysis_Case1();
                var result = runTimeCase.Run(0); // This will throw a DivideByZeroException
            }));

        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero()
        {
            var sourceCode = ExceptionAnalyzed.PrepareSourceCodeFileForAnalysis(@"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\RunTimeAnalysis_Case1.cs");
            var promptStr = $@"
Analyze the following C# file name ""RunTimeAnalysis_Case1.cs"", for the following Exception: ""System.DivideByZeroException""
on line 15.

C# File RunTimeAnalysis_Case1.cs:
{sourceCode}
";
            var client = new OpenAI();
            var prompt = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful and experienced C# and .NET software developer." },
                    new GPTMessage { Role =  MessageRole.user, Content = promptStr }
                },
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            var t = response.Text;
        }
    }
}



