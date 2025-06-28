using ChatGPT.Tests.CSRunTimeErrorAnalysis;
using DynamicSugar;
using fAI;
using fAI.SourceCodeAnalysis;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
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
using S = System;

/*
 
using c# and the openai completion api https://api.openai.com/v1/chat/completions
how to pass a file to be analyzed for runtime errors?

 */
namespace fAI.Tests
{

    public class  FLogViewerClient
    {
        public const string LofgViewerCommunicationFileName = @"C:\temp\flogviewer.notification.txt";

        public static bool RequestToOpenFile(string fileName)
        {
            try
            {
                if (File.Exists(LofgViewerCommunicationFileName))
                    File.Delete(LofgViewerCommunicationFileName);
                File.WriteAllText(LofgViewerCommunicationFileName, fileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class CodeAnalysisTests : OpenAIUnitTestsBase
    {
        public CodeAnalysisTests()
        {
            OpenAI.TraceOn = true;
        }


        [Fact()]
        public void RunTimeAnalysis_CodeAnalysis_Anthropic()
        {
            var ea = new CodeAnalysis(nameof(RunTimeAnalysis_CodeAnalysis_Anthropic),
                new FileLocation
                {
                    FileName = @"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\CodeAnalysis_Cases\CodeAnalysis_Case1.cs",
                    ClassName = "MathHelper",
                    MethodName = "GetAverage",
                    LineNumber = 9
                });

            var analysisReportFileName = ea.AnalyzeCodeAndGenerateReport();
        }

        [Fact()]
        public void RunTimeAnalysis_CodeAnalysis_ClosureInLoop_Anthropic()
        {
            var ea = new CodeAnalysis(nameof(RunTimeAnalysis_CodeAnalysis_Anthropic),
                new FileLocation
                {
                    FileName = @"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\CodeAnalysis_Cases\CodeAnalysis_Case2.cs",
                    ClassName = "ButtonCreator",
                    MethodName = "CreateButtons",
                    LineNumber = 10
                });

            var analysisReportFileName = ea.AnalyzeCodeAndGenerateReport();
        }

        [Fact()]
        public void RunTimeAnalysis_CodeAnalysis_IncorrectUseOfValueTypeStructAndAssumingReferenceSemantics_Anthropic()
        {
            var ea = new CodeAnalysis(nameof(RunTimeAnalysis_CodeAnalysis_Anthropic),
                new FileLocation
                {
                    FileName = @"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\CodeAnalysis_Cases\CodeAnalysis_Case3.cs",
                    ClassName = "Program",
                    MethodName = "Main",
                    LineNumber = 19
                });

            var analysisReportFileName = ea.AnalyzeCodeAndGenerateReport();
        }
        
        [Fact()]
        public void RunTimeAnalysis_CodeAnalysis_LinqDeferredExecution_Anthropic()
        {
            var ea = new CodeAnalysis(nameof(RunTimeAnalysis_CodeAnalysis_Anthropic),
                new FileLocation
                {
                    FileName = @"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\CodeAnalysis_Cases\CodeAnalysis_Case4.cs",
                    ClassName = "Program2",
                    MethodName = "Main",
                    LineNumber = 19
                });

            var analysisReportFileName = ea.AnalyzeCodeAndGenerateReport();

            FLogViewerClient.RequestToOpenFile(analysisReportFileName);
        }
    }
}




