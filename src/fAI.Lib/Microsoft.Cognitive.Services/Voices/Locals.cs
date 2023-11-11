using System.Collections.Generic;
using System.Linq;

namespace fAI
{
    public class Locals : List<Local>
    {
        public static class LanguagesSupported
        {
            public const string English = "English";
            public const string French = "French";
            public const string Spanish = "Spanish";
        }

        public void Sort()
        {
            var l = new List<Local>();
            l.AddRange(this);
            l.OrderBy(x => x.Locale);
            this.Clear();
            this.AddRange(l);
        }

    }
}

