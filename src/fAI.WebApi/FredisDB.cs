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

        public bool AddUpdateFileName(string type, string fileName, string json)
        {
            return _fRedis.SetKey(GetFileNameKey(type, fileName), json, 0);
        }

        public bool DeleteFileName(string type, string filename)
        {
            if (GetFileName(type, filename) == null)
                return false;

            _fRedis.DeleteKey(GetFileNameKey(type, filename));

            return true;
        }

        public string GetFileName(string type, string fileName)
        {
            var pid = CreatePid(fileName);
            var json = _fRedis.Get<string>($"{FREDIS_DB_PREFIX}{type}-{pid}");
            if (string.IsNullOrEmpty(json))
                return null;

            return json;
        }
    }
}

