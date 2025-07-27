using Fredis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace fAI.WebApi
{
    public class CheckListItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string imageUrl { get; set; }
        public bool completed { get; set; }
    }

    public class CheckList
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<CheckListItem> items { get; set; }


        public static CheckList SampleCheckList()
        {
            return new CheckList
            {
                id = "sample-checklist",
                title = "Sample Checklist",
                items = new List<CheckListItem>
                {
                    new CheckListItem { id = "1", title = "Item 1", imageUrl = "https://example.com/image1.jpg", completed = false },
                }
            };
        }

        public CheckList()
        {
            items = new List<CheckListItem>();
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static CheckList FromJSON(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<CheckList>(json);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public class FredisDB
    {
        public string url => Environment.GetEnvironmentVariable("fredis_url");
        public string password = Environment.GetEnvironmentVariable("fredis_password");
        public bool ssl = true;
        public int timeOut = 10;

        public const string FREDIS_DB_PREFIX = "fredis-";

        protected FredisManager _fRedis = null;

        public static ulong StringToUInt64(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Take the first 8 bytes to construct a UInt64
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(hashBytes, 0, 8); // Convert to big-endian if needed
                }

                return BitConverter.ToUInt64(hashBytes, 0);
            }
        }

        public static ulong CreatePid(string pptxFileName)
        {
            var strPID = Path.GetFileNameWithoutExtension(pptxFileName);
            var pid = StringToUInt64(strPID);
            return pid;
        }

        public FredisDB()
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("Fredis URL is not set in environment variables.");

            _fRedis = new FredisManager(url, password, ssl, timeOut);
        }


        public string GetFileNameKey(string type, string filename)
        {
            return $"{FREDIS_DB_PREFIX}{type}-{filename}";
        }
        const string CHECKLIST_TYPE = "checklist";

        public bool AddUpdateCheckList(string fileName, CheckList checkList)
        {
            return _fRedis.SetKey(GetFileNameKey(CHECKLIST_TYPE, fileName), checkList.ToJSON(), 0);
        }

        public bool DeleteCheckList(string filename)
        {
            if (GetCheckList(filename) == null)
                return false;

            _fRedis.DeleteKey(GetFileNameKey(CHECKLIST_TYPE, filename));

            return true;
        }

        public CheckList GetCheckList(string fileName)
        {
            var json = _fRedis.Get<string>(GetFileNameKey(CHECKLIST_TYPE, fileName));
            if (string.IsNullOrEmpty(json))
                return CheckList.SampleCheckList();
            return CheckList.FromJSON(json);
        }
    }
}

