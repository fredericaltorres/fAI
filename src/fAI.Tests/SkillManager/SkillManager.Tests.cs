using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class SkillManagerTests : UnitTestBase
    {
        private const string TestSkillsPath = @".\TestFiles\Skills";

        #region Constructor Tests

        [Fact()]
        public void Constructor_WithValidPath_InitializesSuccessfully()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.True(Directory.Exists(skillManager.RootPath), $"Root path does not exist: {skillManager.RootPath}");
        }

        [Fact()]
        public void Constructor_WithDefaultPath_UsesDefaultSkillsFolder()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new SkillManager());
        }

        [Fact()]
        public void Constructor_WithNullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SkillManager(null));
        }

        [Fact()]
        public void Constructor_WithEmptyPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SkillManager(""));
        }

        [Fact()]
        public void Constructor_WithWhitespacePath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SkillManager("   "));
        }

        [Fact()]
        public void Constructor_WithNonExistentPath_ThrowsDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new SkillManager(@".\NonExistentFolder"));
        }

        #endregion

        #region ListSkills Tests

        [Fact()]
        public void ListSkills_ReturnsExpectedSkills()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ListSkills();

            Assert.NotNull(skills);
            Assert.Equal(2, skills.Count);
            Assert.Contains("DataAnalysisAndInsights", skills);
            Assert.Contains("WordDocumentGeneration", skills);
        }

        [Fact()]
        public void ListSkills_ReturnsSortedList()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ListSkills();

            var sortedSkills = skills.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            Assert.Equal(sortedSkills, skills);
        }

        [Fact()]
        public void ListSkills_ReturnsReadOnlyList()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ListSkills();

            Assert.IsAssignableFrom<IReadOnlyList<string>>(skills);
        }

        #endregion

        #region SkillExists Tests

        [Fact()]
        public void SkillExists_WithExistingSkill_ReturnsTrue()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.True(skillManager.SkillExists("DataAnalysisAndInsights"));
        }

        [Fact()]
        public void SkillExists_WithNonExistentSkill_ReturnsFalse()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.False(skillManager.SkillExists("NonExistentSkill"));
        }

        [Fact()]
        public void SkillExists_WithNullSkillName_ReturnsFalse()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.False(skillManager.SkillExists(null));
        }

        [Fact()]
        public void SkillExists_WithEmptySkillName_ReturnsFalse()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.False(skillManager.SkillExists(""));
        }

        [Fact()]
        public void SkillExists_WithWhitespaceSkillName_ReturnsFalse()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.False(skillManager.SkillExists("   "));
        }

        [Fact()]
        public void SkillExists_IsCaseInsensitive()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.True(skillManager.SkillExists("dataanalysisandinsights"));
            Assert.True(skillManager.SkillExists("DATAANALYSISANDINSIGHTS"));
            Assert.True(skillManager.SkillExists("DataAnalysisAndInsights"));
        }

        #endregion

        #region ReadSkill Tests

        [Fact()]
        public void ReadSkill_WithValidSkill_ReturnsContent()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var content = skillManager.ReadSkill("DataAnalysisAndInsights");

            Assert.NotNull(content);
            Assert.NotEmpty(content);
            Assert.Contains("Data Analysis And Insights", content);
            Assert.Contains("Overview", content);
        }

        [Fact()]
        public void ReadSkill_WithDifferentSkill_ReturnsDifferentContent()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var content = skillManager.ReadSkill("WordDocumentGeneration");

            Assert.NotNull(content);
            Assert.NotEmpty(content);
            Assert.Contains("Word Document Generation", content);
            Assert.Contains("docx", content);
        }

        [Fact()]
        public void ReadSkill_WithNonExistentSkill_ThrowsSkillNotFoundException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var ex = Assert.Throws<SkillNotFoundException>(() => skillManager.ReadSkill("NonExistentSkill"));
            Assert.Equal("NonExistentSkill", ex.SkillName);
        }

        [Fact()]
        public void ReadSkill_WithNullSkillName_ThrowsArgumentException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkill(null));
        }

        [Fact()]
        public void ReadSkill_WithEmptySkillName_ThrowsArgumentException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkill(""));
        }

        #endregion

        #region ReadSkills Tests

        [Fact()]
        public void ReadSkills_WithMultipleValidSkills_ReturnsDictionary()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ReadSkills("DataAnalysisAndInsights", "WordDocumentGeneration");

            Assert.NotNull(skills);
            Assert.Equal(2, skills.Count);
            Assert.True(skills.ContainsKey("DataAnalysisAndInsights"));
            Assert.True(skills.ContainsKey("WordDocumentGeneration"));
            Assert.Contains("Data Analysis And Insights", skills["DataAnalysisAndInsights"]);
            Assert.Contains("Word Document Generation", skills["WordDocumentGeneration"]);
        }

        [Fact()]
        public void ReadSkills_WithSingleSkill_ReturnsDictionary()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ReadSkills("DataAnalysisAndInsights");

            Assert.NotNull(skills);
            Assert.Single(skills);
            Assert.True(skills.ContainsKey("DataAnalysisAndInsights"));
        }

        [Fact()]
        public void ReadSkills_IsCaseInsensitive()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var skills = skillManager.ReadSkills("dataanalysisandinsights");

            Assert.NotNull(skills);
            Assert.Single(skills);
            Assert.True(skills.ContainsKey("dataanalysisandinsights"));
        }

        [Fact()]
        public void ReadSkills_WithNullArray_ThrowsArgumentException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkills(null));
        }

        [Fact()]
        public void ReadSkills_WithEmptyArray_ThrowsArgumentException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkills());
        }

        [Fact()]
        public void ReadSkills_WithNonExistentSkill_ThrowsSkillNotFoundException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<SkillNotFoundException>(() =>
                skillManager.ReadSkills("DataAnalysisAndInsights", "NonExistentSkill"));
        }

        #endregion

        #region ReadAllSkills Tests

        [Fact()]
        public void ReadAllSkills_ReturnsAllSkills()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var allSkills = skillManager.ReadAllSkills();

            Assert.NotNull(allSkills);
            Assert.Equal(2, allSkills.Count);
            Assert.True(allSkills.ContainsKey("DataAnalysisAndInsights"));
            Assert.True(allSkills.ContainsKey("WordDocumentGeneration"));
            Assert.Contains("Data Analysis And Insights", allSkills["DataAnalysisAndInsights"]);
            Assert.Contains("Word Document Generation", allSkills["WordDocumentGeneration"]);
        }

        #endregion

        #region ReadSkillsCombined Tests

        [Fact()]
        public void ReadSkillsCombined_WithDefaultSeparator_CombinesSkills()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var combined = skillManager.ReadSkillsCombined(new[] { "DataAnalysisAndInsights", "WordDocumentGeneration" });

            Assert.NotNull(combined);
            Assert.Contains("# SKILL: Data Analysis And Insights", combined);
            Assert.Contains("# SKILL: Word Document Generation (docx)", combined);
            Assert.Contains("\n\n---\n\n", combined);
        }

        [Fact()]
        public void ReadSkillsCombined_WithCustomSeparator_UsesCustomSeparator()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var combined = skillManager.ReadSkillsCombined(
                new[] { "DataAnalysisAndInsights", "WordDocumentGeneration" },
                "\n@@@\n");

            Assert.NotNull(combined);
            Assert.Contains("\n@@@\n", combined);
        }

        [Fact()]
        public void ReadSkillsCombined_WithSingleSkill_NoSeparator()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var combined = skillManager.ReadSkillsCombined(new[] { "DataAnalysisAndInsights" });

            Assert.NotNull(combined);
            Assert.Contains("# SKILL: Data Analysis And Insights", combined);
        }

        [Fact()]
        public void ReadSkillsCombined_WithNullSkillNames_Throw()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkillsCombined(null));
            
        }

        [Fact()]
        public void ReadSkillsCombined_WithEmptyArray_Throw()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<ArgumentException>(() => skillManager.ReadSkillsCombined(new string[] { }));
        }

        #endregion

        #region GetSkillInfo Tests

        [Fact()]
        public void GetSkillInfo_WithValidSkill_ReturnsInfo()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var info = skillManager.GetSkillInfo("DataAnalysisAndInsights");

            Assert.NotNull(info);
            Assert.Equal("DataAnalysisAndInsights", info.Name);
            Assert.NotNull(info.FilePath);
            Assert.True(info.SizeBytes > 0);
            Assert.True(info.LastModified > DateTime.MinValue);
            Assert.Contains("SKILL.md", info.FilePath);
        }

        [Fact()]
        public void GetSkillInfo_WithNonExistentSkill_ThrowsSkillNotFoundException()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.Throws<SkillNotFoundException>(() => skillManager.GetSkillInfo("NonExistentSkill"));
        }

        [Fact()]
        public void GetSkillInfo_FileSizeIsAccurate()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var info = skillManager.GetSkillInfo("DataAnalysisAndInsights");

            var fileInfo = new FileInfo(info.FilePath);
            Assert.Equal(fileInfo.Length, info.SizeBytes);
        }

        #endregion

        #region GetAllSkillInfos Tests

        [Fact()]
        public void GetAllSkillInfos_ReturnsAllSkillInfos()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var infos = skillManager.GetAllSkillInfos();

            Assert.NotNull(infos);
            Assert.Equal(2, infos.Count);
            Assert.Contains(infos, i => i.Name == "DataAnalysisAndInsights");
            Assert.Contains(infos, i => i.Name == "WordDocumentGeneration");
            Assert.All(infos, i => Assert.True(i.SizeBytes > 0));
        }

        [Fact()]
        public void GetAllSkillInfos_ReturnsReadOnlyList()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            var infos = skillManager.GetAllSkillInfos();

            Assert.IsAssignableFrom<IReadOnlyList<SkillManager.SkillInfo>>(infos);
        }

        #endregion

        #region RootPath Tests

        [Fact()]
        public void RootPath_ReturnsAbsolutePath()
        {
            var skillManager = new SkillManager(TestSkillsPath);
            Assert.True(Path.IsPathRooted(skillManager.RootPath));
        }

        #endregion
    }
}