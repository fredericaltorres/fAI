using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static fAI.SkillManager;

namespace fAI
{

    /*
     https://agentskills.io/what-are-skills
     */
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
    //public class SkillFileInfo
    //{
    //    public SkillFileInfo()
    //    {
    //    }

    //    public string Name { get; set; }
    //    public string FilePath { get; set; }
    //    public long SizeBytes { get; set; }
    //    public DateTime LastModified { get; set; }

    //    public SkillFile LoadSkill()
    //    {
    //        return SkillFile.LoadSkillMd(FilePath);
    //    }
    //}

    public partial class SkillManager
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

        public SkillFileInfo ReadSkill(string skillName)
        {
            var path = ResolveSkillPath(skillName);
            return GetSkillInfo(path);
        }

        public Dictionary<string, SkillFileInfo> ReadSkills(params string[] skillNames)
        {
            ValidateSkillNames(skillNames);
            var result = new Dictionary<string, SkillFileInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in skillNames)
                result[name] = ReadSkill(name);

            return result;
        }

        public Dictionary<string, SkillFileInfo> ReadAllSkills()
        {
            var skillNames = ListSkills();
            return ReadSkills(skillNames.ToArray());
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

   

        public SkillFileInfo GetSkillInfo(string skillName)
        {
            var path = ResolveSkillPath(skillName);
            var fileInfo = new FileInfo(path);

            return new SkillFileInfo
            {
                Name = skillName,
                FilePath = path,
                SizeBytes = fileInfo.Length,
                LastModified = fileInfo.LastWriteTimeUtc
            };
        }

        public IReadOnlyList<SkillFileInfo> GetAllSkillInfos()
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

            if (File.Exists(skillName))
                return skillName;

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