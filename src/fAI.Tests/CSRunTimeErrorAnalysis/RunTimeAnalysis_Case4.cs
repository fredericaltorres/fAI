using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicSugar;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    internal class RunTimeAnalysis_Case4
    {
        public int Run(int input)
        {
            using (var tfh = new TestFileHelper())
            {
                var tmpTxtFile = tfh.GetTempFileName(".txt");
                var fs = new FileStream(tmpTxtFile, FileMode.Append, FileAccess.Write, FileShare.None);
                var writer = new StreamWriter(fs);
                    writer.WriteLine("First line");

                var fsRead = new FileStream(tmpTxtFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // Read the contents of the file using fsRead
                fsRead.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(fsRead, Encoding.UTF8, true, 1024, leaveOpen: true))
                {
                    string fileContent = reader.ReadToEnd();
                    Console.WriteLine(fileContent);
                }
                fsRead.Close();

                return 1;
            }
        }
    }
}