using fAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fAI
{
    public class ExecuteProgramUtilty
    {
        public static bool ExecProgram(string program, string parameters)
        {
            int intExitCode = 0;
            return ExecProgram(program, parameters, ref intExitCode);
        }
        public static bool ExecProgram(string program, string parameters, ref int intExitCode)
        {
            return ExecProgram(program, parameters, ref intExitCode, false, false, null).Succeeded;
        }
        public class ExecProgramResponse
        {
            public bool Succeeded { get; set; }
            public Exception Exception { get; set; }
            public string Output;
            public int ProcessId;
        }

        public static ExecProgramResponse ExecProgram(string program, string parameters, ref int intExitCode, bool sameProcess, bool booHidden, Action notifyCaller = null, int sleepTime = 2, bool captureOutput = false)
        {
            var capturedOutput = new StringBuilder();
            var r = new ExecProgramResponse();
            try
            {
                Process proc;
                if (sameProcess)
                    proc = System.Diagnostics.Process.GetCurrentProcess();
                else
                    proc = new System.Diagnostics.Process();

                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = program;
                proc.StartInfo.Arguments = parameters;

                if (booHidden)
                {
                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }

                if (captureOutput)
                {
                    proc.StartInfo.UseShellExecute = false;
                    proc.EnableRaisingEvents = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        try { capturedOutput.AppendLine(e.Data); } catch { }
                    };

                    proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                        try { capturedOutput.AppendLine(e.Data); } catch { }
                    };
                }
                
                proc.Start();

                if (captureOutput)
                {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                
                    while(!proc.HasExited)
                    {
                        Thread.Sleep(sleepTime*1000);
                        if(notifyCaller != null)
                            notifyCaller();
                    }
                    r.Output = capturedOutput.ToString();
                    if (notifyCaller != null)
                        notifyCaller();
                }
                else if(captureOutput ==  false && notifyCaller != null)
                {
                    while (!proc.HasExited)
                    {
                        Thread.Sleep(sleepTime * 1000);
                        if (notifyCaller != null)
                            notifyCaller();
                    }
                    if (notifyCaller != null)
                        notifyCaller();
                }
                else
                {
                    proc.WaitForExit(); // Block and wait
                }
                intExitCode = proc.ExitCode;
                r.Succeeded = true;
            }
            catch(System.Exception ex)
            {
                r.Succeeded = false;
                r.Exception = ex;
            }
            return r;
        }


        public static ExecProgramResponse ExecProgramAsynchronously(string program, string parameters)
        {
            var capturedOutput = new StringBuilder();
            var r = new ExecProgramResponse();
            try
            {
                Process proc = new System.Diagnostics.Process();

                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = program;
                proc.StartInfo.Arguments = parameters;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                proc.Start();
                Thread.Sleep(2 * 1000);
                r.ProcessId = proc.Id;
                r.Succeeded = true;
            }
            catch (System.Exception ex)
            {
                r.Succeeded = false;
                r.Exception = ex;
            }
            return r;
        }
    }
}
