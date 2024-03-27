﻿using System;
using System.Collections.Generic;

namespace fAI
{
    public class Groq : Logger
    {

        public Groq(int timeOut = -1)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;
        }
        
        public GroqCompletions _completions = null;
        public GroqCompletions Completions => _completions ?? (_completions = new GroqCompletions());
    }
}
