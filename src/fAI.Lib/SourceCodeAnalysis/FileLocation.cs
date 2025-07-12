using DynamicSugar;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace fAI.SourceCodeAnalysis
{
    public class FileLocation
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public override string ToString()
        {
            return $"LocalFileFound:{LocalFileFound}, {MethodName}, {FileName}:{LineNumber}";
        }

        public static string RemoveDoubleBackSlash(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Replace(@"\\", @"\");
        }

        [JsonIgnore]
        public Dictionary<string, string> PathReplace = new Dictionary<string, string>
        {
            { @"E:\b\master\",  @"C:\brainshark\development\core\" },
            { @"E:\JenkinsAgent\workspace\monitor-jobs_master@2\",  @"C:\brainshark\development\Monitors\monitor.core\" },
        };

        public bool LocalFileFound => !string.IsNullOrEmpty(FileName) && System.IO.File.Exists(GetLocalFileName());

        public void Clean()
        {
            FileName = FileLocation.RemoveDoubleBackSlash(FileName);
            var parts = this.MethodName?.Split(DS.List(" at ").ToArray(), 1024, System.StringSplitOptions.None);
            this.MethodName = parts?.Length > 0 ? parts.Last() : this.MethodName;
        }

        public string GetLocalFileName()
        {
            if (string.IsNullOrEmpty(FileName))
                return FileName;

            var localFileName = FileName;
            foreach (var item in PathReplace)
            {
                if (localFileName.ToLowerInvariant().StartsWith(item.Key.ToLowerInvariant()))
                {
                    localFileName = item.Value + localFileName.Substring(item.Key.Length);
                    break;
                }
            }
            return localFileName;
        }
    }
}




