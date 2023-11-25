using System.IO;
using DynamicSugar;

namespace fAI
{
    public partial class StaticHtmlTemplateGenerator : TemplateGenerator
    {

        public string Header => base.GetStaticFile("StaticHtmlTemplateGenerator.Header.html");
        public string Footer => base.GetStaticFile("StaticHtmlTemplateGenerator.Footer.html");
        public string DocumentTemplate => base.GetStaticFile("StaticHtmlTemplateGenerator.Document.html");


        public override string GenerateFooter()
        {
            return this.Footer;
        }

        public override string GenerateHeader()
        {
            return this.Header;
        }

        public override string GenerateTemplate(GeneratedDocumentDetail gdd)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(
                DocumentTemplate.Template(new { gdd.Title, gdd.Summary, gdd.LocalImage })
                );
            return sb.ToString();
        }
    }
}

