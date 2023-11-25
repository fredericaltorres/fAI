using System.IO;

namespace fAI
{
    public abstract class TemplateGenerator
    {
        public abstract string GenerateHeader();
        public abstract string GenerateFooter();

        public abstract string GenerateTemplate(GeneratedDocumentDetail gdd);
        public string GenerateTemplate(GeneratedDocuments generatedDocuments)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var gdd in generatedDocuments)
            {
                sb.AppendLine(this.GenerateTemplate(gdd));
            }
            return sb.ToString();
        }


        public string GetStaticFile(string fileName)
        {
            return File.ReadAllText($@"GeneratedDocument\{fileName}");
        }
    }
}

