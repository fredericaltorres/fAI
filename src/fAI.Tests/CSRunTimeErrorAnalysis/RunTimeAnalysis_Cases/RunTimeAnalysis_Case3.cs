using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    internal class RunTimeAnalysis_Case3
    {
        private int GetNumberFromConfig()
        {
            var number = System.Configuration.ConfigurationManager.AppSettings["Number"];
            if (!string.IsNullOrWhiteSpace(number))
            {
                return int.Parse(number);
            }
            return 0; // Default value if not found or empty
        }

        public int Number { get { return GetNumberFromConfig(); } }

        public int Run(int input)
        {
            return input / Number;
        }
    }
}
