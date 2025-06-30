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
DeliveryRequest 'donotdelete.txt' not found. at Brainshark.Brainshark.Reporting.DeliveryRequest..ctor(String id)
 at Brainshark.Monitors.ReportingService.DeliveryMonitor.Process()
 in E:\JenkinsAgent\workspace\monitor-jobs_master@2\Brainshark.Monitors.ReportingService\DeliveryMonitor.cs:line 140 
rt=Jun 29 2025 23:59:27 start=Jun 29 2025 23:59:27 end=Jun 29 2025 23:59:27 dvchost=bos3bkndsvc02|┊

";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException1);
            Assert.Equal(@"System.Exception", ea.ExceptionType);
            Assert.Equal(@"DeliveryRequest 'donotdelete.txt' not found", ea.Message);
            Assert.Single(ea.StackTraceInfo);
            Assert.Equal(@"Brainshark.Monitors.ReportingService.DeliveryMonitor.Process()", ea.StackTraceInfo[0].MethodName);
            Assert.Equal(@"E:\JenkinsAgent\workspace\monitor-jobs_master@2\Brainshark.Monitors.ReportingService\DeliveryMonitor.cs", ea.StackTraceInfo[0].FileName);
            Assert.Equal(140, ea.StackTraceInfo[0].LineNumber);
        }
    }
}




