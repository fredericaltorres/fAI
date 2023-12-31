﻿using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;

namespace fAI
{
    public class JsonUtils
    {
        public static string ToJSON(object o, Formatting formatting = Formatting.Indented)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o, formatting);
        }

        public static T FromJSON<T>(string json)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception ex)
            {
                throw new System.ApplicationException($"Cannot deserialize json error:{ex.Message}, json:{json}", ex);
            }
        }
    }
}


