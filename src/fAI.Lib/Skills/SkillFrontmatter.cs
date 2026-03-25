using System;
using System.IO;

namespace fAI
{

public partial class SkillManager
    {
        public class SkillFrontmatter
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public static SkillFrontmatter ParseFrontmatter(string yaml)
            {
                var result = new SkillFrontmatter();

                foreach (var line in yaml.Split('\n'))
                {
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex < 0) continue;

                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    switch (key.ToLower())
                    {
                        case "name": result.Name = value; break;
                        case "description": result.Description = value; break;
                    }
                }

                return result;
            }

            public static SkillFrontmatter LoadSkillFrontmatter(string filePath)
            {
                string content = File.ReadAllText(filePath);
                string[] parts = content.Split(new string[] { "---" }, 3, StringSplitOptions.None);

                if (parts.Length < 3)
                    throw new FormatException("No valid YAML frontmatter found in skill file.");

                return ParseFrontmatter(parts[1]);
            }
        }
    }
}