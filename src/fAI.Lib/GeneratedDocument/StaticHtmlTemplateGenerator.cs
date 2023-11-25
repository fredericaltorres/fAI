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
            var s = base.Template(DocumentTemplate, new { gdd.Title, gdd.Summary, gdd.LocalImage, gdd._summaryPrompt, gdd._imagePrompt });
            s = s.Replace(@"\r\n", "<BR />");
            return s;
        }
    }
}

