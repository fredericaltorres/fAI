using CsvHelper;
using CsvHelper.Configuration;
using fAI.Microsoft.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI
{
    public class EssaiAICsvMapping : ClassMap<EssaiAI>
    {
        public EssaiAICsvMapping()
        {
            Map(m => m.Url).Name("Url");
            Map(m => m.Title).Name("Text");
        }
    }

    public class PresentationAICsvMapping : ClassMap<PresentationAI>
    {
        public PresentationAICsvMapping()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.Title).Name("Text");
            Map(m => m.Description).Name("Description");
        }
    }

    public class CSVMLoader
    {
        public List<T> Load<T, Tmap>(string fileName, Tmap modelClassMap) 
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
            };
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<PresentationAICsvMapping>();
                    return csv.GetRecords<T>().ToList();
                }
            }
        }
    }


    public class CSVEssaiAILoader
    {
        public List<EssaiAI> Load(string fileName)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
            };
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<EssaiAICsvMapping>();
                    return csv.GetRecords<EssaiAI>().ToList();
                }
            }
        }
    }
}
