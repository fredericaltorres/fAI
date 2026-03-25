using Markdig;
using System;
using System.IO;

namespace fAI
{

public partial class SkillManager
    {
        public class SkillFile
        {
            public SkillFrontmatter Frontmatter { get; set; }
            public string MarkdownBody { get; set; }

            public static SkillFile LoadSkillMd(string filePath)
            {
                string content = File.ReadAllText(filePath);
                string[] parts = content.Split(new string[] { "---" }, 3, StringSplitOptions.None);

                if (parts.Length < 3)
                    throw new FormatException("No valid YAML frontmatter found in skill file.");

                SkillFrontmatter frontmatter = SkillFrontmatter.ParseFrontmatter(parts[1]);

                string markdownBody = parts[2].Trim();
                //string renderedHtml = Markdown.ToHtml(markdownBody);

                return new SkillFile
                {
                    Frontmatter = frontmatter,
                    MarkdownBody = markdownBody,
                };
            }
        }
    }
}