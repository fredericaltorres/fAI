using System;

namespace fAI
{

    public class Local
    {
        public string Locale { get; set; }
        public string LocaleName { get; set; }
        public string ShortName { get; set; }

        public override string ToString()
        {
            return LocaleName;
        }
    }
}

