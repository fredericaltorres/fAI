// -------------------------------------------------------------------------------------------------------------
// LogViewer.net Plug In
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// Copyright 2008 Frederic Torres
// All rights reserved
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Regarding the fLogViewer.net software see the End User License Agreement.
//
// -------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Web;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;

namespace faiWinApp
{
    public class Executorr
    {
        public static bool ExplorerOpenInContainingFolder(string fileName)
        {
            return ExecProgram("EXPLORER.EXE", string.Format("/select,\"{0}\"", fileName), false);
        }

        public static bool ExecFile(string strFile)
        {
            try
            {
                var proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = strFile;
                proc.StartInfo.Arguments = "";
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ExecProgram(string strProgram, string strParameter, bool booWait)
        {
            int intExitCode = 0;
            return ExecProgram(strProgram, strParameter, booWait, ref intExitCode);
        }
        public static bool ExecProgram(string strProgram, string strParameter, bool booWait, ref int intExitCode)
        {
            return ExecProgram(strProgram, strParameter, booWait, ref intExitCode, false);
        }
        public static bool ExecProgram(string strProgram, string strParameter, bool booWait, ref int intExitCode, bool booSameProcess)
        {
            try
            {
                System.Diagnostics.Process proc;
                if (booSameProcess)
                    proc = System.Diagnostics.Process.GetCurrentProcess();
                else
                    proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = strProgram;
                proc.StartInfo.Arguments = strParameter;
                proc.Start();
                Application.DoEvents();
                if (booWait)
                {
                    proc.WaitForExit();
                    intExitCode = proc.ExitCode;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public class ExecutionInfo
        {
            public string Output;
            public string ErrorOutput;
            public int Time;
            public string CommandLine;
            public int ErrorLevel;

            public bool Succeeded
            {
                get
                {
                    return this.ErrorLevel == 0;
                }
            }

            public ExecutionInfo()
            {

                Output = "";
                ErrorOutput = "";
                Time = -1;
                CommandLine = "";
                ErrorLevel = -1;
            }
        }

        public static ExecutionInfo ExecutePowerShellScriptInVisibleConsole(string script, string parameters = null)
        {
            var r = new ExecutionInfo();
            int exitCode = -1;
            var cmd = script;
            if (parameters != null)
                cmd += $" {parameters}"; // < Should we escape the " with `"
            var rr = ExecProgram("powershell.exe", $@"-command ""{cmd}"" ", false, ref exitCode, false);
            r.ErrorLevel = rr ? 0 : -1;
            return r;
        }

        const string PowerShellExe64B = @"C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe";
        public static ExecutionInfo ExecutePowerShellScriptAndCapture(string script)
        {
            return ExecuteConsoleAndCapture(PowerShellExe64B, $@"-command ""{script}"" ");
            //var is64 = Environment.Is64BitProcess;
            //return ExecuteConsoleAndCapture(@"C:\Brainshark\development\poc\cheetah.regression.tool\Brainshark.DataAccess.CommandLine\bin\Debug\Brainshark.DataAccess.CommandLine.exe",
            //    $@"getPresentation --pid 794613385");
        }

        //public static ExecutionInfo ExecuteConsoleViaOS(string program, string commandLine)
        //{
        //    var e = new ExecutionInfo();
        //    e.CommandLine = program + " " + commandLine;
        //    e.Time = Environment.TickCount;
        //    e.ErrorLevel = -1;
        //    try
        //    {
        //        var processStartInfo = new ProcessStartInfo(program, commandLine);
        //        processStartInfo.ErrorDialog = false;
        //        processStartInfo.UseShellExecute = true;
        //        //processStartInfo.CreateNoWindow = true;
        //        processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //        var process = new Process();
        //        process.StartInfo = processStartInfo;
        //        bool processStarted = process.Start();
        //        if (processStarted)
        //        {
        //            process.WaitForExit();
        //            e.ErrorLevel = process.ExitCode;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        e.ErrorOutput += string.Format("Error lanching the {0} = {1}", e.CommandLine, ex.ToString());
        //    }
        //    finally
        //    {
        //        if (!string.IsNullOrWhiteSpace(e.ErrorOutput) && e.ErrorLevel == 0)
        //        {
        //            // Signal an internal error during the execution for example running a powershell script which run 2 EXEs the second failing
        //            // The powershell execution succeeded per say so error level is 0 but internall the 2 exe outputed an error
        //            e.ErrorLevel = -2;
        //        }

        //        if (e.ErrorLevel != 0)
        //        {
        //            e.ErrorOutput += $"ErrorLevel:{e.ErrorLevel}";
        //            e.Output = "";
        //        }

        //    }
        //    e.Time = Environment.TickCount - e.Time;
        //    return e;
        //}

        public static ExecutionInfo ExecuteConsoleAndCapture(string program, string commandLine)
        {
            var capturedOutput = new StringBuilder();
            var capturedError = new StringBuilder();
            ExecutionInfo e = new ExecutionInfo();
            e.CommandLine = program + " " + commandLine;
            e.Time = Environment.TickCount;
            e.ErrorLevel = -1;
            try
            {
                Process process = System.Diagnostics.Process.GetCurrentProcess();
                process.StartInfo.UseShellExecute = false;
                process.EnableRaisingEvents = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs ee)
                {
                    try { capturedOutput.AppendLine(ee.Data); } catch { }
                };

                process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs ee) {
                    try { capturedError.AppendLine(ee.Data); } catch { }
                };

                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = commandLine;
                process.StartInfo.CreateNoWindow = true;

                bool processStarted = process.Start();

                if (processStarted)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    while (!process.HasExited)
                    {
                        Thread.Sleep(1000);
                    }
                    e.ErrorLevel = process.ExitCode;
                    e.Output = capturedOutput.ToString();
                    e.ErrorOutput = capturedError.ToString();
                }
            }
            catch (Exception ex)
            {
                e.ErrorOutput += string.Format("Error lanching the {0} = {1}", e.CommandLine, ex.ToString());
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(e.ErrorOutput) && e.ErrorLevel == 0)
                {
                    // Signal an internal error during the execution for example running a powershell script which run 2 EXEs the second failing
                    // The powershell execution succeeded per say so error level is 0 but internall the 2 exe outputed an error
                    e.ErrorLevel = -2;
                }

                if (e.ErrorLevel != 0)
                {
                    e.ErrorOutput += $"{Environment.NewLine}{e.Output}";
                    e.Output = "";
                }
            }
            e.Time = Environment.TickCount - e.Time;
            return e;
        }
    }
}
