using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace fAI
{
	public class Models
	{
		public string @object { get; set; }
		public List<Datum> data { get; set; }

		public static Models FromJSON(string json)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<Models>(json);
		}

		public class Datum
		{
			public string id { get; set; }
			public string @object { get; set; }
			public int created { get; set; }
            public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(created).DateTime; // https://www.epochconverter.com/
            public string owned_by { get; set; }
			public List<Permission> permission { get; set; }
			public string root { get; set; }
			public object parent { get; set; }
		}

		public class Permission
		{
			public string id { get; set; }
			public string @object { get; set; }
			public int created { get; set; }
            public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(created).DateTime;
            public bool allow_create_engine { get; set; }
			public bool allow_sampling { get; set; }
			public bool allow_logprobs { get; set; }
			public bool allow_search_indices { get; set; }
			public bool allow_view { get; set; }
			public bool allow_fine_tuning { get; set; }
			public string organization { get; set; }
			public object group { get; set; }
			public bool is_blocking { get; set; }
		}
	}
}
