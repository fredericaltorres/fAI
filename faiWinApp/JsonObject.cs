namespace faiWinApp
{
    public class JsonObject
    {
        public string FileName { get; set; }

        public void ToFile(string file = null)
        {
            if (file == null)
                file = this.FileName;
            System.IO.File.WriteAllText(file, ToJSON());
        }
        protected string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static AppOptions FromFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                var r = FromJSON(System.IO.File.ReadAllText(file));
                r.FileName = file;
                return r;
            }
            else
            {
                var r = new AppOptions();
                r.FileName = file;
                return r;
            }
                
        }

        public static AppOptions FromJSON(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppOptions>(json);
        }
    }
}
