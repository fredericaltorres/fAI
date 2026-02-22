using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fAI
{

    /// <summary>
    /// Manages SKILL.md files organized under a root "skills" folder.
    /// 
    /// Expected folder structure:
    ///   skills/
    ///     docx/
    ///       SKILL.md
    ///     pdf/
    ///       SKILL.md
    ///     imagegen/
    ///       SKILL.md
    ///     ...
    /// </summary>
    public class SkillManager
    {
        private readonly string _rootPath;
        public string RootPath => _rootPath;
        private const string SkillFileName = "SKILL.md";

        public SkillManager(string rootPath = @".\skills")
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Root path must not be null or empty.", nameof(rootPath));

            _rootPath = Path.GetFullPath(rootPath);

            if (!Directory.Exists(_rootPath))
                throw new DirectoryNotFoundException($"skills directory not found: {_rootPath}");
        }

        public string ReadSkill(string skillName)
        {
            var path = ResolveSkillPath(skillName);
            return File.ReadAllText(path);
        }

        public Dictionary<string, string> ReadSkills(params string[] skillNames)
        {
            ValidateSkillNames(skillNames);

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in skillNames)
                result[name] = ReadSkill(name);

            return result;
        }

        public Dictionary<string, string> ReadAllSkills()
        {
            var skillNames = ListSkills();
            return ReadSkills(skillNames.ToArray());
        }

        public string ReadSkillsCombined(IEnumerable<string> skillNames, string separator = null)
        {
            if(skillNames == null)
                throw new ArgumentException("Skill names collection must not be null.", nameof(skillNames));
            if (skillNames.ToList().Count == 0)
                throw new ArgumentException("At least one skill name must be provided.", nameof(skillNames));

            separator = separator == null ? "\n\n---\n\n" : separator;
            var names = skillNames?.ToArray() ?? Array.Empty<string>();
            ValidateSkillNames(names);

            var builder = new StringBuilder();
            for (int i = 0; i < names.Length; i++)
            {
                var content = ReadSkill(names[i]);
                //builder.Append($"## Skill: {names[i]}\n\n");
                builder.Append(content);

                if (i < names.Length - 1)
                    builder.Append(separator);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns the names of all skill folders that contain a SKILL.md file.
        /// </summary>
        public IReadOnlyList<string> ListSkills()
        {
            EnsureRootExists();
            return Directory
                .GetDirectories(_rootPath)
                .Where(dir => File.Exists(Path.Combine(dir, SkillFileName)))
                .Select(dir => Path.GetFileName(dir))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Returns true if the given skill folder exists and contains a SKILL.md.
        /// </summary>
        public bool SkillExists(string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName)) return false;

            var path = Path.Combine(_rootPath, skillName, SkillFileName);
            return File.Exists(path);
        }

        public class SkillInfo
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public long SizeBytes { get; set; }
            public DateTime LastModified { get; set; }
        }

        public SkillInfo GetSkillInfo(string skillName)
        {
            var path = ResolveSkillPath(skillName);
            var fileInfo = new FileInfo(path);

            return new SkillInfo
            {
                Name = skillName,
                FilePath = path,
                SizeBytes = fileInfo.Length,
                LastModified = fileInfo.LastWriteTimeUtc
            };
        }

        public IReadOnlyList<SkillInfo> GetAllSkillInfos()
        {
            return ListSkills()
                .Select(GetSkillInfo)
                .ToList();
        }



        // -------------------------------------------------------------------------
        // Private Helpers
        // -------------------------------------------------------------------------

        private string ResolveSkillPath(string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
                throw new ArgumentException("Skill name must not be null or empty.", nameof(skillName));

            EnsureRootExists();

            var skillFolder = Path.Combine(_rootPath, skillName);
            if (!Directory.Exists(skillFolder))
                throw new SkillNotFoundException(skillName, $"Skill folder not found: {skillFolder}");

            var skillFile = Path.Combine(skillFolder, SkillFileName);
            if (!File.Exists(skillFile))
                throw new SkillNotFoundException(skillName, $"SKILL.md not found in: {skillFolder}");

            return skillFile;
        }

        private void EnsureRootExists()
        {
            if (!Directory.Exists(_rootPath))
                throw new DirectoryNotFoundException($"Skills root folder not found: {_rootPath}");
        }

        private static void ValidateSkillNames(string[] names)
        {
            if (names == null || names.Length == 0)
                throw new ArgumentException("At least one skill name must be provided.");
        }
    }


    /// <summary>
    /// Thrown when a requested skill or its SKILL.md file cannot be found.
    /// </summary>
    public class SkillNotFoundException : Exception
    {
        public string SkillName { get; }

        public SkillNotFoundException(string skillName, string message)
            : base(message)
        {
            SkillName = skillName;
        }

        public SkillNotFoundException(string skillName, string message, Exception innerException)
            : base(message, innerException)
        {
            SkillName = skillName;
        }
    }
}