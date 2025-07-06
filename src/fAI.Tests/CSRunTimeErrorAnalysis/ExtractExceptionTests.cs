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


        const string LogException3 = @"
  85 ┊ 2025/07/05 04:45:24.014 PM ┊ 2025/07/05 04:45:28.457 PM ┊ bos3webbsk04   ┊ prod/webbsk/app_logs     ┊ 2025-07-05 16:45:24.014|Brainshark|Core|1.0.1.0|ERROR|bos3webbsk04|msg=CEF:0|Brainshark|Core|0|Message|Message|Error|msg=[ERROR, 4.0s] RSUtilities.SaveReportFromWizard(). userId: 0; wizardPath: /Brainshark Reports/Presentation Reports/Viewing Details by Viewer; reportName: Viewing Details by Viewer; format: CSV; , Message: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server., Exception: System.Net.WebException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. ---> System.IO.IOException: Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host. ---> System.Net.Sockets.SocketException: An existing connection was forcibly closed by the remote host     at System.Net.Sockets.Socket.Receive(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags)     at System.Net.Sockets.NetworkStream.Read(Byte[] buffer, Int32 offset, Int32 size)     --- End of inner exception stack trace ---     at System.Net.Sockets.NetworkStream.Read(Byte[] buffer, Int32 offset, Int32 size)     at System.Net.PooledStream.Read(Byte[] buffer, Int32 offset, Int32 size)     at System.Net.Connection.SyncRead(HttpWebRequest request, Boolean userRetrievedStream, Boolean probeRead)     --- End of inner exception stack trace ---     at System.Web.Services.Protocols.WebClientProtocol.GetWebResponse(WebRequest request)     at System.Web.Services.Protocols.HttpWebClientProtocol.GetWebResponse(WebRequest request)     at System.Web.Services.Protocols.SoapHttpClientProtocol.Invoke(String methodName, Object[] parameters)     at Brainshark.Brainshark.Reporting.ReportService2005.ReportingService2005.SetItemDataSources(String Item, DataSource[] DataSources) in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\Web References\\ReportService2005\\Reference.cs:line 1924     at Brainshark.Brainshark.Reporting.RS2008.RS2008Service.SetDefaultDataSource(String reportPath) in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\RS2008\\RS2008Service.cs:line 309     at Brainshark.Brainshark.Reporting.RSUtilities.<>c__DisplayClass84_1.<SaveReportFromWizard>b__5() in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\RSUtilities.cs:line 1962     at Brainshark.Brainshark.Reporting.RSUtilities.TraceExecution(String message, Action action, Object properties) in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\RSUtilities.cs:line 165     at Brainshark.Brainshark.Reporting.RSUtilities.<>c__DisplayClass84_0.<SaveReportFromWizard>b__0() in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\RSUtilities.cs:line 1961     at Brainshark.Brainshark.Reporting.RSUtilities.TraceExecution[T](String message, Func`1 callBack, Object properties, Boolean traceStart) in E:\\b\\master\\Code\\DotNet Beta\\Brainshark\\Brainshark.Brainshark.Reporting\\RSUtilities.cs:line 183 rt=Jul 05 2025 16:45:24 start=Jul 05 2025 16:45:24 end=Jul 05 2025 16:45:24 dvchost=bos3webbsk04|sid=|cid=|uid=|pid=|errorCode=-1|errorMessage=NO_ERROR_CODE_PROVIDED|url=|

";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog3()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException3);
            Assert.Equal(@"System.Net.WebException", ea.ExceptionType);
            Assert.Equal(@"The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. ---> System.IO.IOException: Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host. ---> System.Net.Sockets.SocketException: An existing connection was forcibly closed by the remote host", ea.Message);
            Assert.Equal(6, ea.StackTraceInfo.Count);

            var sb = new StringBuilder();

            foreach (var st in ea.StackTraceInfo)
            {
                Assert.True(st.LocalFileFound);
                /////sb.AppendLine($@"  Assert.Equal(@""{st.FileName}"", ea.StackTraceInfo[x++].FileName); ");
                ////sb.AppendLine($@"  Assert.Equal({st.LineNumber}, ea.StackTraceInfo[x++].LineNumber); ");
            }
            var s = sb.ToString();

            var x = 0;
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\Web References\ReportService2005\Reference.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\RS2008\RS2008Service.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\RSUtilities.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\RSUtilities.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\RSUtilities.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\b\master\Code\DotNet Beta\Brainshark\Brainshark.Brainshark.Reporting\RSUtilities.cs", ea.StackTraceInfo[x++].FileName);

            x = 0;
            Assert.Equal(1924, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(309, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(1962, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(165, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(1961, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(183, ea.StackTraceInfo[x++].LineNumber);

            var otherFiles = ea.StackTraceInfo.Select(st => st.GetLocalFileName()).ToList();
            otherFiles.RemoveAt(0);
            ea.OtherFiles = DS.List(otherFiles[0]); // Add only the second file. If we add all the file the prompt is too long.

            ea.Case = "ExtractDotNetExceptionFromLog3";
            ea.JsonFileName =  ExceptionAnalyzer.GetJsonFileName(ea.Case);
            ea.Save(ea.JsonFileName);

            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }



        const string LogException4 = @"
    79 ┊ 2025/07/05 05:31:34.773 PM ┊ 2025/07/05 05:31:38.063 PM ┊ bos3acvtcert02 ┊ prod/cert/app_logs       ┊ 2025-07-05 17:31:34.773|Brainshark|Core|1.0.1.0|ERROR|bos3acvtcert02|CEF:0|Brainshark|Core|0|Message|Message|Error|msg=BrainsharkMonitorService on BOS3ACVTCERT02,Error,CertificateConversionMonitor,JobId:486480717, UserId:0, CompanyId:0, CourseId:, Exception: System.ApplicationException: Error replacing tag in certificate offset:151, text:[This is to certify that Patience  Antwi  Patience  Antwi  Congratulations on Completing Interpreting Cardiac Rhythm Strips  Total Score:  0   ], callId:B     at Brainshark.Monitors.Common.CertificateConverterMonitor.VerifyStringOffset(String text, Int32 offset, String callId) in E:\\JenkinsAgent\\workspace\\monitor-jobs_master@2\\Brainshark.Monitors.Common\\CertificateConverterMonitor.cs:line 1275     at Brainshark.Monitors.Common.CertificateConverterMonitor.ReplaceShapeText2(IShape oShp, Curriculum oCurriculum, CurriculumEnrollment oCurriculumEnrollment, Course oCourse, Enrollment oCourseEnrollment, User oAuthor, Presentation oPresentation, User oUser, ViewData oViewData, Company oCompany, String sCertificateMessage, Double lPresentationScoreAchieved, Int32 jobId) in E:\\JenkinsAgent\\workspace\\monitor-jobs_master@2\\Brainshark.Monitors.Common\\CertificateConverterMonitor.cs:line 1212 rt=Jul 05 2025 17:31:34 start=Jul 05 2025 17:31:34 end=Jul 05 2025 17:31:34 dvchost=bos3acvtcert02|

";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog4()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException4);
            Assert.Equal(@"System.ApplicationException", ea.ExceptionType);
            Assert.Equal(@"Error replacing tag in certificate offset:151, text:[This is to certify that Patience  Antwi  Patience  Antwi  Congratulations on Completing Interpreting Cardiac Rhythm Strips  Total Score:  0   ], callId:B", ea.Message);
            Assert.Equal(2, ea.StackTraceInfo.Count);

            var sb = new StringBuilder();

            foreach (var st in ea.StackTraceInfo)
            {
                Assert.True(st.LocalFileFound);
                ///////sb.AppendLine($@"  Assert.Equal(@""{st.FileName}"", ea.StackTraceInfo[x++].FileName); ");
                sb.AppendLine($@"  Assert.Equal({st.LineNumber}, ea.StackTraceInfo[x++].LineNumber); ");
            }
            var s = sb.ToString();

            var x = 0;
            Assert.Equal(@"E:\JenkinsAgent\workspace\monitor-jobs_master@2\Brainshark.Monitors.Common\CertificateConverterMonitor.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\JenkinsAgent\workspace\monitor-jobs_master@2\Brainshark.Monitors.Common\CertificateConverterMonitor.cs", ea.StackTraceInfo[x++].FileName);
            x = 0;
            Assert.Equal(1275, ea.StackTraceInfo[x++].LineNumber);
            Assert.Equal(1212, ea.StackTraceInfo[x++].LineNumber);

            var otherFiles = ea.StackTraceInfo.Select(st => st.GetLocalFileName()).ToList();
            otherFiles.RemoveAt(0);
            ea.OtherFiles = DS.List(otherFiles[0]); // Add only the second file. If we add all the file the prompt is too long.
            if(Path.GetFileName(ea.SourceCodeFileName) == Path.GetFileName(ea.OtherFiles[0]))
                ea.OtherFiles = DS.List<string>();

            ea.Case = "ExtractDotNetExceptionFromLog3";
            ea.JsonFileName = ExceptionAnalyzer.GetJsonFileName(ea.Case);
            ea.Save(ea.JsonFileName);

            var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }
    }
}
