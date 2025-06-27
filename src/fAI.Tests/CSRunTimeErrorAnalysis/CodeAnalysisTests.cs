using ChatGPT.Tests.CSRunTimeErrorAnalysis;
using DynamicSugar;
using fAI;
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
using Xunit;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;
using fAI.SourceCodeAnalysis;

/*
 
using c# and the openai completion api https://api.openai.com/v1/chat/completions
how to pass a file to be analyzed for runtime errors?

 */
namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class CodeAnalysisTests : OpenAIUnitTestsBase
    {
        public CodeAnalysisTests()
        {
            OpenAI.TraceOn = true;
        }


        [Fact()]
        [TestBeforeAfter]
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
    }
}




