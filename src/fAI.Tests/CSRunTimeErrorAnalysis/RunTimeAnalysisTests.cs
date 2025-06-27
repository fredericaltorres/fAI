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
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case1().Run(1); // This will throw a DivideByZeroException
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);
            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Prompt_GPT_4o());
        }


        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitializationInConfigFile()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case2().Run(1); // This will throw a DivideByZeroException
            }), otherFiles: DS.List(@"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\app.config"));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);
            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Prompt_GPT_4o());
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitializationInConfigFile_v2()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case3().Run(1); // This will throw a DivideByZeroException
            }), otherFiles: DS.List(@"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\RunTimeAnalysis_Cases\app.config"));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);

            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Prompt_GPT_4o());
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_DivisionByZero_MissingInitializationInConfigFile_v2_Anthropic()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case3().Run(1); // This will throw a DivideByZeroException
            }), otherFiles: DS.List(@"C:\DVT\fAI\src\fAI.Tests\CSRunTimeErrorAnalysis\app.config"));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);
            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_FileLocked_Anthropic()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case4().Run(1);
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);
            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_NullString_Anthropic()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var result = new RunTimeAnalysis_Case5().Run(1);
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);

            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }

        [Fact()]
        [TestBeforeAfter]
        public void RunTimeAnalysis_ModifyingCollectionWhileIteratingOverIt_Anthropic()
        {
            var exceptionDescriptionFileName = ExceptionAnalyzed.RunCode(new S.Action(() =>
            {
                var instance = new RunTimeAnalysis_Case7();
                instance.Items = DS.List("Item1", "Item2", "", "Item3", null, "Item4");
                instance.RemoveEmptyItems(); 
            }));

            var ea = ExceptionAnalyzed.Load(exceptionDescriptionFileName);
            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());

        }
    }
}




