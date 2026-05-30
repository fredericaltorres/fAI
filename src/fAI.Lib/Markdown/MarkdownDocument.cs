using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a parsed Markdown document with its front matter metadata and body content.
/// </summary>
public class MarkdownDocument
{
    public FrontMatter FrontMatter { get; set; }

    public bool HasFrontMatter => FrontMatter != null;

    public string MarkdownBody { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;

    public static string RemoveImgTags(string markdown)
    {
        // 3. HTML <img> tag with double or single quotes
        markdown = Regex.Replace(
            markdown,
            @"<img[^>]*\ssrc\s*=\s*([""'])data:image/[^;]+;base64,[^""']+\1[^>]*\/?>",
            string.Empty,
            RegexOptions.IgnoreCase
        );
        return markdown;
    }

    public static string RemoveBase64Images(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return markdown;

        // Matches ![alt text](data:image/...;base64,...)
        return Regex.Replace(
            markdown,
            @"!\[[^\]]*\]\(data:image[^)]+\)",
            string.Empty,
            RegexOptions.IgnoreCase
        );
    }

    public string RawContentWithoutBase64ImageData
    {
        get {
            return RemoveImgTags(RemoveBase64Images(this.RawContent));
        }
    }

    /// <summary>
    /// Serializes the document (front matter + body) and writes it to disk.
    /// </summary>
    public void Update(string fileName)
    {
        var sb = new StringBuilder();
        if (FrontMatter != null)
            FrontMatter.FrontMatterToText(sb);
        sb.Append(MarkdownBody);
        string content = sb.ToString();

        string directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fileName, content, Encoding.UTF8);
    }
}

/// <summary>
/// Represents the YAML front matter block of a Markdown document.
/// Extend with additional properties as needed for your project.
/// </summary>
public class FrontMatter
{
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Date { get; set; }

    public void SetDateToNow()
    {
        this.Date = DateTime.Now.Date;
    }   

    public void SetDateToUtcNow()
    {
        this.Date = DateTime.UtcNow.Date;
    }


    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> ExtraFields { get; set; } = new Dictionary<string, string>();

    public void FrontMatterToText(StringBuilder sb)
    {
        sb.AppendLine("---");

        if (!string.IsNullOrEmpty(this.Name))
            sb.AppendLine($"name: {this.Name}");

        if (!string.IsNullOrEmpty(this.Title))
            sb.AppendLine($"title: {this.Title}");

        if (!string.IsNullOrEmpty(this.Description))
            sb.AppendLine($"description: {this.Description}");

        if (!string.IsNullOrEmpty(this.Author))
            sb.AppendLine($"author: {this.Author}");

        if (this.Date.HasValue)
            sb.AppendLine($"date: {this.Date.Value:yyyy-MM-dd}");

        if (this.Tags?.Count > 0)
        {
            sb.AppendLine("tags:");
            foreach (string tag in this.Tags)
                sb.AppendLine($"  - {tag}");
        }

        foreach (var kvp in this.ExtraFields ?? new Dictionary<string, string>())
            sb.AppendLine($"{kvp.Key}: {kvp.Value}");

        sb.AppendLine("---");
    }
}

/// <summary>
/// Loads and parses a Markdown file, separating the YAML front matter from the body.
/// </summary>
public static class MarkdownLoader
{
    // Matches a front matter block: starts and ends with "---" on its own line
    private static readonly Regex FrontMatterRegex = new Regex(
        @"^\-{3}\s*\r?\n(?<yaml>.*?)\r?\n\-{3}\s*\r?\n(?<body>.*)",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    public static MarkdownDocument Update(string filePath, string newMarkdownBody, FrontMatter frontMatter = null)
    {
        var document = Load(filePath);
        document.MarkdownBody = newMarkdownBody;
        if (frontMatter != null)
            document.FrontMatter = frontMatter;
        document.Update(filePath);
        return document;
    }

    /// <summary>
    /// Loads a Markdown file from disk and returns a parsed <see cref="MarkdownDocument"/>.
    /// </summary>
    public static MarkdownDocument Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Markdown file not found: {filePath}", filePath);

        string raw = File.ReadAllText(filePath, Encoding.UTF8);
        return Parse(raw);
    }

    /// <summary>
    /// Parses a raw Markdown string into a <see cref="MarkdownDocument"/>.
    /// </summary>
    public static MarkdownDocument Parse(string rawContent)
    {
        var document = new MarkdownDocument { RawContent = rawContent };

        Match match = FrontMatterRegex.Match(rawContent);

        if (match.Success)
        {
            string yaml = match.Groups["yaml"].Value.Trim();
            document.MarkdownBody = match.Groups["body"].Value.Trim();
            document.FrontMatter = ParseFrontMatter(yaml);
        }
        else
        {
            // No front matter found — the entire content is the body
            document.MarkdownBody = rawContent.Trim();
            document.FrontMatter = null;
        }

        return document;
    }

    // -------------------------------------------------------------------------
    // Minimal YAML parser for simple key: value pairs and list items (- value).
    // For complex YAML, swap this out for a library like YamlDotNet.
    // -------------------------------------------------------------------------
    private static FrontMatter ParseFrontMatter(string yaml)
    {
        var frontMatter = new FrontMatter();
        string[] lines = yaml.Split('\n');

        string currentListKey = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd();

            // List item under the current key
            if (line.StartsWith("  - ") || line.StartsWith("- "))
            {
                string item = line.TrimStart().TrimStart('-').Trim();
                if (currentListKey == "tags")
                    frontMatter.Tags.Add(item);
                continue;
            }

            currentListKey = null;

            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            string key = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
            string value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

            switch (key)
            {
                case "name": frontMatter.Name = value; break;
                case "title": frontMatter.Title = value; break;
                case "description": frontMatter.Description = value; break;
                case "author": frontMatter.Author = value; break;
                case "date": DateTime date; if (DateTime.TryParse(value, out date)) frontMatter.Date = date; break;
                case "tags":
                    // Inline list: tags: [foo, bar]
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        string inner = value.Trim('[', ']');
                        foreach (string tag in inner.Split(','))
                            frontMatter.Tags.Add(tag.Trim().Trim('"', '\''));
                    }
                    else
                    {
                        // Multi-line list follows
                        currentListKey = "tags";
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(value))
                        frontMatter.ExtraFields[key] = value;
                    break;
            }
        }

        return frontMatter;
    }
}
