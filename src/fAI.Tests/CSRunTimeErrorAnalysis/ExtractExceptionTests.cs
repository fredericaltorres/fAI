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
Exception:System.ApplicationException: Error processing tag! You are certified in Bla at CConverter.Verify(String text, Int32 offset) in Z:\JAgent\work\Common\Cert.cs:line 1275 at CConverter.Replace(ViewData oViewData, Company oCompany, String sCertificateMessage) in Z:\JAgent\work\Common\Cert.cs:line 1212
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog2()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException2);
            Assert.Equal(@"System.ApplicationException", ea.ExceptionType);
            Assert.Equal(@"Error processing tag! You are certified in Bla", ea.Message);
            Assert.Equal(2, ea.StackTraceInfo.Count);

            Assert.Equal(@"CConverter.Verify(String text, Int32 offset)", ea.StackTraceInfo[0].MethodName);
            Assert.Equal(@"Z:\JAgent\work\Common\Cert.cs", ea.StackTraceInfo[0].FileName);
            Assert.Equal(1275, ea.StackTraceInfo[0].LineNumber);

            Assert.Equal(@"CConverter.Replace(ViewData oViewData, Company oCompany, String sCertificateMessage)", ea.StackTraceInfo[1].MethodName);
            Assert.Equal(@"Z:\JAgent\work\Common\Cert.cs", ea.StackTraceInfo[1].FileName);
            Assert.Equal(1212, ea.StackTraceInfo[1].LineNumber);
        }

        const string LogException3 = @"
  85 ┊ 2025/07/05 04:45:24.014 PM ┊ 2025-07-05 16:45:24.014|Zark||ERROR|Zark|msg=[ERROR, 4.0s] RUtil.SaveReport(). userId: 0; dPath: /Reports. Exception: System.Net.WebException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. ---> System.IO.IOException: Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host. ---> System.Net.Sockets.SocketException: An existing connection was forcibly closed by the remote host     at System.Net.Sockets.Socket.Receive(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags)     at System.Net.Sockets.NetworkStream.Read(Byte[] buffer, Int32 offset, Int32 size)     --- End of inner exception stack trace ---     at System.Net.Sockets.NetworkStream.Read(Byte[] buffer, Int32 offset, Int32 size)     at System.Net.PooledStream.Read(Byte[] buffer, Int32 offset, Int32 size)     at System.Net.Connection.SyncRead(HttpWebRequest request, Boolean userRetrievedStream, Boolean probeRead)     --- End of inner exception stack trace ---     at System.Web.Services.Protocols.WebClientProtocol.GetWebResponse(WebRequest request)     at System.Web.Services.Protocols.HttpWebClientProtocol.GetWebResponse(WebRequest request)     at System.Web.Services.Protocols.SoapHttpClientProtocol.Invoke(String methodName, Object[] parameters)     at Zark.Reporting.ReportService2005.ReportingService2005.SetItemDataSources(String Item, DataSource[] DataSources) in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\Web References\\ReportService2005\\Reference.cs:line 1924     at Zark.Reporting.RS2008.RS2008Service.SetDefaultDataSource(String reportPath) in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\RS2008\\RS2008Service.cs:line 309     at Zark.Reporting.RUtil.<>c__DisplayClass84_1.<SaveReportFromWizard>b__5() in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\RUtil.cs:line 1962     at Zark.Reporting.RUtil.TraceExecution(String message, Action action, Object properties) in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\RUtil.cs:line 165     at Zark.Reporting.RUtil.<>c__DisplayClass84_0.<SaveReportFromWizard>b__0() in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\RUtil.cs:line 1961     at Zark.Reporting.RUtil.TraceExecution[T](String message, Func`1 callBack, Object properties, Boolean traceStart) in E:\\main\\Code\\Zeta\\Zark\\Zark.Reporting\\RUtil.cs:line 183 
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
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\Web References\ReportService2005\Reference.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\RS2008\RS2008Service.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\RUtil.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\RUtil.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\RUtil.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\main\Code\Zeta\Zark\Zark.Reporting\RUtil.cs", ea.StackTraceInfo[x++].FileName);

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

            /////var analysisReportFileName = ea.AnalyzeAndGenerateAnalysisReport(new Anthropic_Prompt_Claude_3_5_Sonnet());
        }



        const string LogException4 = @"
    ZarkRotoSvc,Error,CCRotot,JobId:486480717, UserId:0, CompanyId:0, CourseId:, 
Exception: System.ApplicationException: Error processing tag in certificate offset:151 
 at Zark.Rotors.Common.CConverterRotor.VerifyStringPosition(String text, Int32 offset, String callId)
    in E:\JAgent\work\main\Zark.Rotors.Common\CConverterRotor.cs:line 1275     
 at Zark.Rotors.Common.CConverterRotor.ReplaceShapeText2(IShape oShp, Curriculum oCurriculum, CurriculumEnrollment oCurriculumEnrollment, Course oCourse, Enrollment oCourseEnrollment, User oAuthor, Presentation oPresentation, User oUser, ViewData oViewData, Company oCompany, String sCertificateMessage, Double lPresentationScoreAchieved, Int32 jobId)
    in E:\JAgent\work\main\Zark.Rotors.Common\CConverterRotor.cs:line 1212 blablah
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog4()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException4);
            Assert.Equal(@"System.ApplicationException", ea.ExceptionType);
            Assert.Equal(@"Error processing tag in offset:151", ea.Message);
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
            Assert.Equal(@"E:\JAgent\work\main\Zark.Rotors.Common\CConverterRotor.cs", ea.StackTraceInfo[x++].FileName);
            Assert.Equal(@"E:\JAgent\work\main\Zark.Rotors.Common\CConverterRotor.cs", ea.StackTraceInfo[x++].FileName);
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

        const string LogException5 = @"
aaaa bbbb
Exception: System.Net.WebException: The remote server returned an error: (400) Bad Request.  
 at System.Net.WebClient.UploadDataInternal(Uri address, String method, Byte[] data, WebRequest& request) 
 at System.Net.WebClient.UploadData(Uri address, String method, Byte[] data)   
 at System.Net.WebClient.UploadData(String address, Byte[] data)   
 at Brainshark.Azure.Search.Utility.performSearchQuery(String query, String filter, String searchFields, String sort, Int32 offset, Int32 perPage, String index, String select)
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog5()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException5);
            Assert.Equal(@"System.Net.WebException", ea.ExceptionType);
            Assert.Equal(@"The remote server returned an error: (400) Bad Request.", ea.Message);
            Assert.Equal(4, ea.StackTraceInfo.Count);
            Assert.False(ea.HasFileName);
        }


        const string LogException6 = @"
SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server).
 at System.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)   
 at System.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection) 
 at System.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)    
 at System.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions) 
 at System.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions) 
 at System.Data.SqlClient.SqlConnection.TryOpenInner(TaskCompletionSource`1 retry) 
 at System.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry)    
 at System.Data.SqlClient.SqlConnection.Open()   
 at Brark.DtaSvc.Sql.OpenConnection()================
Inner Exception: Win32Exception: The network path was not found.   blablah

";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractDotNetExceptionFromLog6()
        {
            var ea = ExceptionAnalyzer.ExtractFromLog(LogException6);
            Assert.Equal(@"SqlException", ea.ExceptionType);
            Assert.Equal(@"A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server).", ea.Message);
            Assert.Equal(9, ea.StackTraceInfo.Count);
            Assert.False(ea.HasFileName);
        }
    }
}
