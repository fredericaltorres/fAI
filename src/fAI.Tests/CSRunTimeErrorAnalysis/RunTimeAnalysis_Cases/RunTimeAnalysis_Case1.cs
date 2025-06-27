using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    internal class RunTimeAnalysis_Case1
    {
        public int Number { get; set; }

        public int Run(int input)
        {
            return input / Number;
        }
    }
}
