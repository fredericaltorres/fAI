using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    internal class RunTimeAnalysis_Case7
    {
        public List<string> Items = new List<string>();

        public void RemoveEmptyItems()
        {
            foreach (var item in Items)
            {
                if (string.IsNullOrWhiteSpace(item))
                    Items.Remove(item);
            }
        }
    }
}
