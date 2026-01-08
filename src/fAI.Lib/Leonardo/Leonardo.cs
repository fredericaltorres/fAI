using DynamicSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace fAI
{
    // https://docs.leonardo.ai/reference/getuserself
    public class Leonardo : HttpBase
    {
        public Leonardo(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            Trace("Leonardo ctor", this);
            base._key = Environment.GetEnvironmentVariable("LEONARDO_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (openAiKey != null)
                base._key = openAiKey;
        }
        
    
        public LeonardoImage _image = null;
        public LeonardoImage Image => _image ?? (_image = new LeonardoImage());
    }

}
