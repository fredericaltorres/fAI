using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    internal class RunTimeAnalysis_Case2
    {
        private int GetNumberFromConfig()
        {
            var number = System.Configuration.ConfigurationManager.AppSettings["Number"];
            if (number != null)
                return int.Parse(number);
            return 0; // Default value if not found
        }

        public int Number { get { return GetNumberFromConfig(); } }

        public int Run(int input)
        {
            return input / Number;
        }
    }
}
