using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT.Tests.CSRunTimeErrorAnalysis
{
    public interface IPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
    }
    internal class RunTimeAnalysis_Case6
    {
        public class Person : IPerson
        {

            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public static string FullName(IPerson p)
        {
            return p.FirstName + " " + p.LastName;
        }
    }
}
