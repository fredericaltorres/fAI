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
    public class SkillFileInfo
    {
        public SkillFileInfo()
        {
        }

        public string Name { get; set; }
        public string FilePath { get; set; }
        public long SizeBytes { get; set; }
        public DateTime LastModified { get; set; }

        public SkillFile LoadSkill()
        {
            return SkillFile.LoadSkillMd(FilePath);
        }
    }
}