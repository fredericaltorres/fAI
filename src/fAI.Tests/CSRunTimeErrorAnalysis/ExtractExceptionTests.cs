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

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class ExtractExceptionTests : OpenAIUnitTestsBase
    {
        public ExtractExceptionTests()
        {
            OpenAI.TraceOn = true;
        }


        const string LogException1 = @"
System.Exception: 
RequeteDeLivraison 'NePasDetruire.txt' non trouve. 
 at RrainBar.Rotors.ReportSvc.LivraisonEngine.Run() in Z:\JAgent\space\job\ReportingSvc\LivraisonEngine.cs:line 140 
blah blah

";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog1()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException1);
            Assert.Equal(@"System.Exception", ea.ExceptionType);
            Assert.Equal(@"RequeteDeLivraison 'NePasDetruire.txt' non trouve.", ea.Message);
            Assert.Single(ea.StackTraceInfo);
            Assert.Equal(@"RrainBar.Rotors.ReportSvc.LivraisonEngine.Run()", ea.StackTraceInfo[0].MethodName);
            Assert.Equal(@"Z:\JAgent\space\job\ReportingSvc\LivraisonEngine.cs", ea.StackTraceInfo[0].FileName);
            Assert.Equal(140, ea.StackTraceInfo[0].LineNumber);
        }

        const string LogException2 = @"
Exception:System.ApplicationException: Error replacing tag! You are certified in Bla at CConverter.Verify(String text, Int32 offset) in Z:\JAgent\work\Common\Certificate.cs:line 1275 at CConverter.Replace(ViewData oViewData, Company oCompany, String sCertificateMessage) in Z:\JAgent\work\Common\Certificate.cs:line 1212
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog2()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException2);
            Assert.Equal(@"System.ApplicationException", ea.ExceptionType);
            Assert.Equal(@"Error replacing tag! You are certified in Bla", ea.Message);
            Assert.Equal(2, ea.StackTraceInfo.Count);

            Assert.Equal(@"CConverter.Verify(String text, Int32 offset)", ea.StackTraceInfo[0].MethodName);
            Assert.Equal(@"Z:\JAgent\work\Common\Certificate.cs", ea.StackTraceInfo[0].FileName);
            Assert.Equal(1275, ea.StackTraceInfo[0].LineNumber);

            Assert.Equal(@"CConverter.Replace(ViewData oViewData, Company oCompany, String sCertificateMessage)", ea.StackTraceInfo[1].MethodName);
            Assert.Equal(@"Z:\JAgent\work\Common\Certificate.cs", ea.StackTraceInfo[1].FileName);
            Assert.Equal(1212, ea.StackTraceInfo[1].LineNumber);
        }
    }
}




