using DynamicSugar;
using fAI;
using fAI.OpenAIModel.ImageResponseGpt;
using fAI.Util.Strings;
using HtmlAgilityPack;
using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using static fAI.HumeAISpeech;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenericAI__Completions_UnitTests : OpenAIUnitTestsBase
    {
        //Regex _quickFilter = new Regex(AIMemoryManager.DEFAULT_MODEL_FOR_META_DATA_EXTRACTION);
        //Regex _quickFilter = new Regex("gemini-.*");
        const string DefaultModelToUse = "google/gemini-3.1-flash-lite";
        Regex _quickFilter = new Regex(DefaultModelToUse);
        const int _randomModelCount = 2;

        //Regex _quickFilter = null;

        public GenericAI__Completions_UnitTests()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle_ExperimentMultiMode()
        {
            try
            {
                var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
                var model = "gemini-3.1-flash-lite";
                var expectedWords = DS.List("alice", "insurance", "car");
                var client = new GenericAI();

                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);

                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, ", this);

                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
            }
            catch (Exception ex)
            {
                HttpBase.Trace($"[ERROR]Exception: {ex.Message}", this);
            }
            finally
            {
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
            var expectedWords = DS.List("alice", "insurance", "car");
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model.Id);

                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                HttpBase.Trace($"[SUMMARIZATION] Model: {model.Id}, Duration: {result.Duration:0.0}, ", this);

                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle_ConversationMode_2()
        {
            var text = "What is the capital of France?";
            var expectedWords = DS.List("paris", "france");

            //Regex quickFilter = new Regex("google/gemini-3.1-flash-lite");
            Regex quickFilter = new Regex("openai/gpt-5.6-luna");

            var systemPrompt = @"
you are a geography expert. You will answer questions about countries, capitals, and geography in general.
";

            foreach (var model in GenericAI.GetModels(quickFilter))
            {
                var client = new GenericAI();
                // Conversation step 1
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model.Id, systemPrompt: systemPrompt);
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                
                // Conversation step 2
                var text2 = "What is its population?";

                result = client.Completions.TextImprovement(text: text2, language: "English", model: model.Id, contents: result.Contents);
                Assert.True(DS.List("million", "residents","2").All(w => result.Text.ToLower().Contains(w)));
                
                // Conversation step 3
                var text3 = @"in the last 10 years, is Paris population shrinking? Answer with YES or NO only.";
                result = client.Completions.TextImprovement(text: text3, language: "English", model: model.Id, contents: result.Contents);
                Assert.Contains("yes", result.Text.ToLower());
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle_ConversationMode_1()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
            var expectedWords = DS.List("alice", "insurance", "car");
            //var models = DS.List("gemini-2.0-flash", "claude-sonnet-4-5", "claude-haiku-4-5", "gpt-5-mini");

            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                // Conversation step 1
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model.Id);

                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                Assert.Equal(3, result.Contents.Count); // Query + Response
                Assert.Equal("system", result.Contents[0].Role.ToString());
                Assert.Equal("user", result.Contents[1].Role.ToString());
                Assert.Equal("assistant", result.Contents[2].Role.ToString());
                Assert.Contains(text.Trim(), result.Contents[1].Content[0].Text.Trim());

                // Conversation step 2
                var text2 = @"What is this conversation about?";

                result = client.Completions.TextImprovement(text: text2, language: "English", model: model.Id, contents: result.Contents);
                Assert.True(DS.List("conversation", "insurance").All(w => result.Text.ToLower().Contains(w)));

                Assert.Equal(5, result.Contents.Count); // Query + Response
                Assert.Equal("system", result.Contents[0].Role.ToString());
                Assert.Equal("user", result.Contents[1].Role.ToString());
                Assert.Equal("assistant", result.Contents[2].Role.ToString());
                Assert.Equal("user", result.Contents[3].Role.ToString());
                Assert.Equal("assistant", result.Contents[4].Role.ToString());

                // Conversation step 3
                var text3 = @"is the car insurance proposal approved? Answer with YES or NO only.";
                result = client.Completions.TextImprovement(text: text3, language: "English", model: model.Id, contents: result.Contents);
                Assert.Contains("yes", result.Text.ToLower());

                Assert.Equal(7, result.Contents.Count); // Query + Response
                Assert.Equal("system", result.Contents[0].Role.ToString());
                Assert.Equal("user", result.Contents[1].Role.ToString());
                Assert.Equal("assistant", result.Contents[2].Role.ToString());
                Assert.Equal("user", result.Contents[3].Role.ToString());
                Assert.Equal("assistant", result.Contents[4].Role.ToString());
                Assert.Equal("user", result.Contents[5].Role.ToString());
                Assert.Equal("assistant", result.Contents[6].Role.ToString());
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void SummarizationResult_CountWords()
        {
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This is a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This was, a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords(@"This 
                                                                                  was 
                                                                                  a test."));
        }

        const string GlycemicReseachText = @"
A groundbreaking study published in Cell approximately seven years ago by researchers in Israel, 
titled 'Personalized Nutrition by Prediction of Glycemic Responses', 
generated considerable interest. 
This research effectively demonstrated that individuals can exhibit significantly different glycemic responses 
to the same food, 
even something as simple as a handful of blueberries.
This finding challenges the conventional understanding of the glycemic index, 
which posits a predictable glucose rise based on the quantity of food and its glucose content. 

This is important because sustained glycemic variability over time can negatively impact our health. 
It is beneficial to select or balance foods in a way that promotes greater stability in blood sugar levels.

Therefore, understanding your individual glycemic response to various foods is crucial. Furthermore, 
adopting lifestyle strategies such as improving sleep quality, engaging in post-meal walks, 
incorporating resistance training, and utilizing cold exposure techniques can also contribute to better 
glycemic control and overall well-being.
";

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var expectedWords = DS.List("alice", "insurance", "car");
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.Summarize(text: GlycemicReseachText, language: "English", model: model.Id, summarizeWordCount: 64);
                HttpBase.Trace($"[SUMMARIZATION] Duration: {result.Duration:00.00}, Model: {model.Id}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}", this);
                var cost = client.Completions.LastUsage.ComputeCost();
                Assert.True(cost > 0, $"Cost should be greater than 0 for model {model.Id}");
            }
        }


        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_OpenRouterModels_ToString()
        {
            var modelInfo = OpenRouter.GetModels().Select(model => model.ToString()).ToList();
        }

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_GenericAI_OpenRouterModels()
        {
            var expectedWords = DS.List("alice", "insurance", "car");
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);
            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.Summarize(text: GlycemicReseachText, language: "English", model: model, summarizeWordCount: 64);
                HttpBase.Trace($"[SUMMARIZATION] Duration: {result.Duration:00.00}, Model: {model}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}, text: {result.Text}", this);

                Assert.True(client.Completions.LastUsage.ComputeCost() > 0, $"Cost should be greater than 0 for model {model}");
                Assert.True(client.Completions.LastUsage.ApiCost > 0, $"ApiCost should be greater than 0 for model {model}");
                Assert.True(client.Completions.LastUsage.ComputeCost() >= client.Completions.LastUsage.ApiCost, $"ComputeCost should be greater than or equal to ApiCost for model {model}");
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void SkillTopicQuestion()
        {
            var skill = "\r\n<skill name=\"genealogist\">\r\n# Professional Genealogist Skill\r\n\r\nYOU ARE A board-certified professional genealogist ??? analyzing family relationships, computing kinship, interpreting historical records, and answering ancestry questions.\r\n\r\n---\r\n\r\n## Trigger Conditions\r\n\r\nThis skill activates on any mention of family trees, ancestry, genealogy, lineage, pedigree, family relationships, kinship, heritage, descendants, ancestors, cousins, in-laws, half-siblings, step-relations, adoptive relationships, common ancestors, DNA genealogy, or casual questions like *\"how is X related to Y?\"* or *\"is my mom's cousin's kid my cousin?\"*\r\n\r\n---\r\n\r\n## Core Competencies\r\n\r\n### 1. Relationship Analysis & Computation\r\n\r\nWhen a user describes family members and asks about relationships, the skill follows a systematic approach.\r\n\r\n**Step A ??? Build the family model.**\r\nParse every person and connection the user provides. Identify each individual with a clear label. Note birth, adoption, marriage, and step relationships distinctly ??? they follow different kinship rules.\r\n\r\n**Step B ??? Find the path.**\r\nTo determine how Person A relates to Person B:\r\n\r\n1. Trace upward from A to the Most Recent Common Ancestor (MRCA).\r\n2. Trace upward from B to the same MRCA.\r\n3. Count the generations from A to the MRCA (call this `a`).\r\n4. Count the generations from B to the MRCA (call this `b`).\r\n5. Apply the relationship formula.\r\n\r\n**Step C ??? Name the relationship.**\r\nUse the standard kinship grid:\r\n\r\n| Generations A ??? MRCA | Generations B ??? MRCA | Relationship of B to A        |\r\n| -------------------- | -------------------- | ----------------------------- |\r\n| 1                    | 1                    | Sibling                       |\r\n| 1                    | 2                    | Niece / Nephew                |\r\n| 2                    | 1                    | Aunt / Uncle                  |\r\n| 2                    | 2                    | First cousin                  |\r\n| 2                    | 3                    | First cousin once removed     |\r\n| 3                    | 2                    | First cousin once removed     |\r\n| 3                    | 3                    | Second cousin                 |\r\n| n                    | n                    | (n???1)th cousin                |\r\n| n                    | m (n ??? m)            | min(n,m)???1 th cousin, \\|n???m\\| times removed |\r\n\r\n**General formula:**\r\n\r\n- **Cousin degree** = min(a, b) ??? 1\r\n- **Times removed** = |a ??? b|\r\n- When one value is 1, the relationship is sibling / parent / child (not cousin).\r\n- When one value is 1 and the other is greater than 1, the relationship is aunt-uncle / niece-nephew with a generational qualifier (great-, great-great-, etc.).\r\n\r\n**Step D ??? Explain clearly.**\r\nAlways provide the named relationship in plain language, the chain of connections (e.g., `A ??? A's mother ??? shared grandparent ??? B's father ??? B`), and a note on any ambiguity.\r\n\r\n---\r\n\r\n### 2. Relationship Qualifiers\r\n\r\nThese distinctions matter and users frequently confuse them:\r\n\r\n- **Half-sibling** ??? Share exactly one biological parent.\r\n- **Step-sibling** ??? No shared biological parent; connected through a parent's marriage.\r\n- **In-law** ??? Related through marriage, not blood. Always specify the exact link (e.g., \"spouse's sibling\" = brother/sister-in-law).\r\n- **Double cousin** ??? When two siblings from one family marry two siblings from another, their children are double first cousins (share all four grandparents instead of two).\r\n- **Adoptive relationship** ??? Treat adoptive relationships as full legal and social kin unless the user specifically asks about biological lines.\r\n- **Removal** ??? \"Once removed\" means one generation apart. Always clarify whether B is a generation above or below A.\r\n\r\n---\r\n\r\n### 3. DNA & Genetic Genealogy\r\n\r\nWhen DNA or genetic questions arise:\r\n\r\n- **Shared centiMorgans (cM):** Expected ranges for common relationships ??? full sibling ~2400 cM, first cousin ~850 cM, second cousin ~210 cM. Real values vary.\r\n- **Haplogroups:** Explain what paternal (Y-DNA) and maternal (mtDNA) haplogroups reveal and their limitations.\r\n- **Endogamy & pedigree collapse:** When communities intermarry across generations, shared DNA inflates and relationships appear closer than they are. Flag this when relevant.\r\n- **Autosomal vs. Y-DNA vs. mtDNA:** Clarify which test answers which question.\r\n\r\n---\r\n\r\n### 4. Historical Record Interpretation\r\n\r\nWhen users bring documents, records, or transcriptions:\r\n\r\n- Identify the record type: census, vital record, church register, immigration manifest, military record, probate, land deed.\r\n- Explain what each field typically means in its historical context.\r\n- Flag common pitfalls: name spelling variations, calendar changes (Julian ??? Gregorian), age rounding in censuses, patronymic vs. surname systems.\r\n- Suggest next records to seek based on what was found.\r\n\r\n---\r\n\r\n### 5. Cultural Naming & Kinship Systems\r\n\r\nDifferent cultures define and name kinship differently:\r\n\r\n- **Patronymic systems** (Icelandic, historical Scandinavian, Arabic, many African cultures) ??? Names formed from the parent's name rather than a fixed surname.\r\n- **Bilateral vs. patrilineal vs. matrilineal** ??? Which line(s) a culture emphasizes for inheritance and identity.\r\n- **Clan and moiety systems** ??? How clan membership affects kinship terminology.\r\n- **Kinship terminology systems** ??? Some languages (Chinese, Hawaiian, Sudanese, Eskimo, etc.) group relatives differently than English.\r\n\r\n---\r\n\r\n## Response Guidelines\r\n\r\n### Always Do\r\n\r\n- Show reasoning step by step ??? trace the path through the family explicitly.\r\n- Use visual chain notation when helpful: `A ??? [relationship] ??? B ??? [relationship] ??? C`.\r\n- Offer to draw a family tree diagram when the family structure is complex.\r\n- Distinguish clearly between biological, legal, and social relationships.\r\n- When information is ambiguous, state assumptions and give the answer under each plausible interpretation.\r\n- Use inclusive language ??? families come in all forms.\r\n- Assume biological relationships when the user hasn't specified.\r\n- Always add women's maiden names\r\n- Regarding woman always provide the precise husband name\r\n- Always provide birth and death date of each person mentioned\r\n\r\n### Never Do\r\n\r\n- Conflate half-, step-, and adoptive relationships without noting the distinction.\r\n- Give a bare label without the reasoning chain.\r\n- Overcomplicate with jargon when plain language works.\r\n\r\n---\r\n\r\n## Worked Example\r\n\r\n**User asks:** *\"My mother's brother's daughter ??? what is she to me?\"*\r\n\r\n1. Start from **You**: You ??? your mother (1 generation up).\r\n2. Your mother ??? her brother (sibling, same generation).\r\n3. Her brother ??? his daughter (1 generation down).\r\n4. Path to MRCA: You are **2 generations** from your maternal grandparents. The daughter is also **2 generations** from those same grandparents.\r\n5. Cousin degree = min(2, 2) ??? 1 = **first cousin**.\r\n6. Times removed = |2 ??? 2| = **0**.\r\n7. **Answer: She is your first cousin** (specifically, a maternal first cousin).\r\n\r\n---\r\n\r\n## Handling Complex Families\r\n\r\nFor large or tangled family structures (remarriages, blended families, intermarriages):\r\n\r\n1. Ask the user to list all individuals and their connections ??? or provide a file or chart.\r\n2. Build an internal model mapping each person to their parents, spouses, and children.\r\n3. When computing any relationship, identify all possible paths and note if multiple relationship paths exist (e.g., \"She is both your first cousin and your step-sister\").\r\n4. If pedigree collapse is present (common ancestors appearing multiple times), flag it and explain the implications.\r\n\r\n---\r\n</skill>\r\n";
            var topic = "\r\n\r\n\r\n<topic_context>\r\nFrederic TORRES, Male, Born 1964 in Aix-en-Provence, FRANCE, (AKA Freddy).\nKarin TORRES, Female, Born 1970, Maiden name: McCowan.\nFrederic TORRES is the husband of Karin TORRES.\nKarin TORRES is the wife of Frederic TORRES.\n\nFrederic TORRES is the father of Marie CHAMBAUD.\nMarie CHAMBAUD, Female, Born 1990 in Aix-en-Provence, FRANCE.\nEmma TORRES is the sister of Alice TORRES.\n,Emma TORRES is the sister of Lea TORRES.\n\nAlice TORRES is the sister of Emma TORRES.\n,Alice TORRES is the sister of Lea TORRES.\n\nLea TORRES is the sister of Emma TORRES.\n,Lea TORRES is the sister of Alice TORRES.\n\nFrederic TORRES is the father of Emma TORRES.\nFrederic TORRES is the father of Alice TORRES.\nFrederic TORRES is the father of Lea TORRES.\nEmma TORRES, Female, Born 1992 in Aix-en-Provence, FRANCE.\nAlice TORRES, Female, Born 1998 in Grasse, FRANCE.\nLea TORRES, Female, Born 2004 in Melrose, MA, USA.\nMarie CHAMBAUD is the half-sister of Emma TORRES.\nEmma TORRES is the half-sister of Marie CHAMBAUD.\nMarie CHAMBAUD is the half-sister of Alice TORRES.\nAlice TORRES is the half-sister of Marie CHAMBAUD.\nMarie CHAMBAUD is the half-sister of Lea TORRES.\nLea TORRES is the half-sister of Marie CHAMBAUD.\nKarin TORRES is the mother of Emma TORRES.\nKarin TORRES is the mother of Alice TORRES.\nKarin TORRES is the mother of Lea TORRES.\nManuel TORRES, Male, Born 1925 in Melilla, Spain, Dead 2004 in Aix-en-Provence, FRANCE.\nSimone TORRES, Female, Born 1929 in Marseille, Dead 1999 in Aix-en-Provence, FRANCE, Maiden name: SEMEAC.\nManuel TORRES is the husband of Simone TORRES.\nSimone TORRES is the wife of Manuel TORRES.\nFrederic TORRES is the brother of Thierry TORRES.\n,Frederic TORRES is the brother of Nathalie TORRES.\n\nThierry TORRES is the brother of Frederic TORRES.\n,Thierry TORRES is the brother of Nathalie TORRES.\n\nNathalie TORRES is the sister of Frederic TORRES.\n,Nathalie TORRES is the sister of Thierry TORRES.\n\nManuel TORRES is the father of Frederic TORRES.\nManuel TORRES is the father of Thierry TORRES.\nManuel TORRES is the father of Nathalie TORRES.\nFrederic TORRES, Male, Born 1964 in Aix-en-Provence, FRANCE, (AKA Freddy).\nThierry TORRES, Male, Born 1966 in Aix-en-Provence, FRANCE, AKA Titoune.\nNathalie TORRES, Female, Born 1969 in Aix-en-Provence, FRANCE.\nManuel TORRES is the father of Patricia TORRES.\nPatricia TORRES, Female, Born ~1962.\nPatricia TORRES is the half-sister of Frederic TORRES.\nFrederic TORRES is the half-sister of Patricia TORRES.\nPatricia TORRES is the half-sister of Thierry TORRES.\nThierry TORRES is the half-sister of Patricia TORRES.\nPatricia TORRES is the half-sister of Nathalie TORRES.\nNathalie TORRES is the half-sister of Patricia TORRES.\nSimone TORRES is the mother of Frederic TORRES.\nSimone TORRES is the mother of Thierry TORRES.\nSimone TORRES is the mother of Nathalie TORRES.\nUnknown is the mother of Patricia TORRES.\nAntoine TORRES, Male, Born 1889, Dead 1953.\nRose TORRES, Female, Born 1904, Dead 1967, Maiden name: CORTES.\nAntoine TORRES is the husband of Rose TORRES.\nRose TORRES is the wife of Antoine TORRES.\nManuella TORRES is the sister of Carmen TORRES.\n,Manuella TORRES is the sister of Raymond Michel TORRES.\n,Manuella TORRES is the sister of Louis TORRES.\n,Manuella TORRES is the sister of Jean-Joseph TORRES.\n,Manuella TORRES is the sister of Augustin TORRES.\n,Manuella TORRES is the sister of Antoine TORRES II.\n\nCarmen TORRES is the sister of Manuella TORRES.\n,Carmen TORRES is the sister of Raymond Michel TORRES.\n,Carmen TORRES is the sister of Louis TORRES.\n,Carmen TORRES is the sister of Jean-Joseph TORRES.\n,Carmen TORRES is the sister of Augustin TORRES.\n,Carmen TORRES is the sister of Antoine TORRES II.\n\nRaymond Michel TORRES is the brother of Manuella TORRES.\n,Raymond Michel TORRES is the brother of Carmen TORRES.\n,Raymond Michel TORRES is the brother of Louis TORRES.\n,Raymond Michel TORRES is the brother of Jean-Joseph TORRES.\n,Raymond Michel TORRES is the brother of Augustin TORRES.\n,Raymond Michel TORRES is the brother of Antoine TORRES II.\n\nLouis TORRES is the brother of Manuella TORRES.\n,Louis TORRES is the brother of Carmen TORRES.\n,Louis TORRES is the brother of Raymond Michel TORRES.\n,Louis TORRES is the brother of Jean-Joseph TORRES.\n,Louis TORRES is the brother of Augustin TORRES.\n,Louis TORRES is the brother of Antoine TORRES II.\n\nJean-Joseph TORRES is the brother of Manuella TORRES.\n,Jean-Joseph TORRES is the brother of Carmen TORRES.\n,Jean-Joseph TORRES is the brother of Raymond Michel TORRES.\n,Jean-Joseph TORRES is the brother of Louis TORRES.\n,Jean-Joseph TORRES is the brother of Augustin TORRES.\n,Jean-Joseph TORRES is the brother of Antoine TORRES II.\n\nAugustin TORRES is the brother of Manuella TORRES.\n,Augustin TORRES is the brother of Carmen TORRES.\n,Augustin TORRES is the brother of Raymond Michel TORRES.\n,Augustin TORRES is the brother of Louis TORRES.\n,Augustin TORRES is the brother of Jean-Joseph TORRES.\n,Augustin TORRES is the brother of Antoine TORRES II.\n\nAntoine TORRES II is the brother of Manuella TORRES.\n,Antoine TORRES II is the brother of Carmen TORRES.\n,Antoine TORRES II is the brother of Raymond Michel TORRES.\n,Antoine TORRES II is the brother of Louis TORRES.\n,Antoine TORRES II is the brother of Jean-Joseph TORRES.\n,Antoine TORRES II is the brother of Augustin TORRES.\n\nAntoine TORRES is the father of Manuella TORRES.\nAntoine TORRES is the father of Carmen TORRES.\nAntoine TORRES is the father of Raymond Michel TORRES.\nAntoine TORRES is the father of Louis TORRES.\nAntoine TORRES is the father of Jean-Joseph TORRES.\nAntoine TORRES is the father of Augustin TORRES.\nAntoine TORRES is the father of Antoine TORRES II.\nManuella TORRES, Female, Born 1932 in Berkane, Morocco.\nCarmen TORRES, Female, Born 1943 in Berkane.\nRaymond Michel TORRES, Male, Born 1928 in Berkane.\nLouis TORRES, Male, Born 1940 in Berkane.\nJean-Joseph TORRES, Male, Born 1935 in Berkane, Dead 1959 Algeria-war.\nAugustin TORRES, Male, Born 1932 in Berkane, Dead ~1935.\nAntoine TORRES II, Male, Born 1937.\nAntoine TORRES II is the brother of Manuella TORRES.\n,Antoine TORRES II is the brother of Carmen TORRES.\n,Antoine TORRES II is the brother of Raymond Michel TORRES.\n,Antoine TORRES II is the brother of Louis TORRES.\n,Antoine TORRES II is the brother of Jean-Joseph TORRES.\n,Antoine TORRES II is the brother of Augustin TORRES.\n\nManuella TORRES is the sister of Antoine TORRES II.\n,Manuella TORRES is the sister of Carmen TORRES.\n,Manuella TORRES is the sister of Raymond Michel TORRES.\n,Manuella TORRES is the sister of Louis TORRES.\n,Manuella TORRES is the sister of Jean-Joseph TORRES.\n,Manuella TORRES is the sister of Augustin TORRES.\n,Manuella TORRES is the sister of Antoine TORRES II.\n\nCarmen TORRES is the sister of Antoine TORRES II.\n,Carmen TORRES is the sister of Manuella TORRES.\n,Carmen TORRES is the sister of Raymond Michel TORRES.\n,Carmen TORRES is the sister of Louis TORRES.\n,Carmen TORRES is the sister of Jean-Joseph TORRES.\n,Carmen TORRES is the sister of Augustin TORRES.\n,Carmen TORRES is the sister of Antoine TORRES II.\n\nRaymond Michel TORRES is the brother of Antoine TORRES II.\n,Raymond Michel TORRES is the brother of Manuella TORRES.\n,Raymond Michel TORRES is the brother of Carmen TORRES.\n,Raymond Michel TORRES is the brother of Louis TORRES.\n,Raymond Michel TORRES is the brother of Jean-Joseph TORRES.\n,Raymond Michel TORRES is the brother of Augustin TORRES.\n,Raymond Michel TORRES is the brother of Antoine TORRES II.\n\nLouis TORRES is the brother of Antoine TORRES II.\n,Louis TORRES is the brother of Manuella TORRES.\n,Louis TORRES is the brother of Carmen TORRES.\n,Louis TORRES is the brother of Raymond Michel TORRES.\n,Louis TORRES is the brother of Jean-Joseph TORRES.\n,Louis TORRES is the brother of Augustin TORRES.\n,Louis TORRES is the brother of Antoine TORRES II.\n\nJean-Joseph TORRES is the brother of Antoine TORRES II.\n,Jean-Joseph TORRES is the brother of Manuella TORRES.\n,Jean-Joseph TORRES is the brother of Carmen TORRES.\n,Jean-Joseph TORRES is the brother of Raymond Michel TORRES.\n,Jean-Joseph TORRES is the brother of Louis TORRES.\n,Jean-Joseph TORRES is the brother of Augustin TORRES.\n,Jean-Joseph TORRES is the brother of Antoine TORRES II.\n\nAugustin TORRES is the brother of Antoine TORRES II.\n,Augustin TORRES is the brother of Manuella TORRES.\n,Augustin TORRES is the brother of Carmen TORRES.\n,Augustin TORRES is the brother of Raymond Michel TORRES.\n,Augustin TORRES is the brother of Louis TORRES.\n,Augustin TORRES is the brother of Jean-Joseph TORRES.\n,Augustin TORRES is the brother of Antoine TORRES II.\n\nAntoine TORRES II is the brother of Manuella TORRES.\n,Antoine TORRES II is the brother of Carmen TORRES.\n,Antoine TORRES II is the brother of Raymond Michel TORRES.\n,Antoine TORRES II is the brother of Louis TORRES.\n,Antoine TORRES II is the brother of Jean-Joseph TORRES.\n,Antoine TORRES II is the brother of Augustin TORRES.\n\nRose TORRES is the mother of Antoine TORRES II.\nRose TORRES is the mother of Manuella TORRES.\nRose TORRES is the mother of Carmen TORRES.\nRose TORRES is the mother of Raymond Michel TORRES.\nRose TORRES is the mother of Louis TORRES.\nRose TORRES is the mother of Jean-Joseph TORRES.\nRose TORRES is the mother of Augustin TORRES.\nAntoine TORRES II, Male, Born 1937.\nManuella TORRES, Female, Born 1932 in Berkane, Morocco.\nCarmen TORRES, Female, Born 1943 in Berkane.\nRaymond Michel TORRES, Male, Born 1928 in Berkane.\nLouis TORRES, Male, Born 1940 in Berkane.\nJean-Joseph TORRES, Male, Born 1935 in Berkane, Dead 1959 Algeria-war.\nAugustin TORRES, Male, Born 1932 in Berkane, Dead ~1935.\nAntoine TORRES II, Male, Born 1937.\nThierry TORRES, Male, Born 1966 in Aix-en-Provence, FRANCE, AKA Titoune.\nAnne-Sophie TORRES, Female, Born 1968 in Aix-en-Provence, FRANCE.\nThierry TORRES is the husband of Anne-Sophie TORRES.\nAnne-Sophie TORRES is the wife of Thierry TORRES.\nTheo TORRES is the brother of Luc TORRES.\n,Theo TORRES is the brother of Lou TORRES.\n\nLuc TORRES is the brother of Theo TORRES.\n,Luc TORRES is the brother of Lou TORRES.\n\nLou TORRES is the sister of Theo TORRES.\n,Lou TORRES is the sister of Luc TORRES.\n\nThierry TORRES is the father of Theo TORRES.\nThierry TORRES is the father of Luc TORRES.\nThierry TORRES is the father of Lou TORRES.\nTheo TORRES, Male, Born 1966 in Aix-en-Provence, FRANCE.\nLuc TORRES, Male, Born 1968 in Aix-en-Provence, FRANCE.\nLou TORRES, Female, Born 1970 in Aix-en-Provence, FRANCE.\nAnne-Sophie TORRES is the mother of Theo TORRES.\nAnne-Sophie TORRES is the mother of Luc TORRES.\nAnne-Sophie TORRES is the mother of Lou TORRES.\nDavid CARRIERE, Male, Born 1969 in Aix-en-Provence, FRANCE.\nNathalie TORRES, Female, Born 1969 in Aix-en-Provence, FRANCE.\nDavid CARRIERE is the husband of Nathalie TORRES.\nNathalie TORRES is the wife of David CARRIERE.\nDavid CARRIERE and Nathalie TORRES are divorced.\nBaptiste CARRIERE is the brother of Paul CARRIERE.\n\nPaul CARRIERE is the brother of Baptiste CARRIERE.\n\nDavid CARRIERE is the father of Baptiste CARRIERE.\nDavid CARRIERE is the father of Paul CARRIERE.\nBaptiste CARRIERE, Male, Born 1969 in Aix-en-Provence, FRANCE.\nPaul CARRIERE, Male, Born 1969 in Aix-en-Provence, FRANCE.\nNathalie TORRES is the mother of Baptiste CARRIERE.\nNathalie TORRES is the mother of Paul CARRIERE.\nJean-Claude X, Male.\nFrederique CHAMBAUD, Female.\nJean-Claude X is the husband of Frederique CHAMBAUD.\nFrederique CHAMBAUD is the wife of Jean-Claude X.\nFrederique CHAMBAUD is the mother of Alexandra CHAMBAUD.\nFrederique CHAMBAUD is the mother of Marie CHAMBAUD.\nAlexandra CHAMBAUD, Female.\nMarie CHAMBAUD, Female, Born 1990 in Aix-en-Provence, FRANCE.\nJean-Claude X is the father of Alexandra CHAMBAUD.\nAlexandra CHAMBAUD, Female.\nJean-Claude X is not the father of Marie CHAMBAUD.\nAlexandra CHAMBAUD is the half-sister of Marie CHAMBAUD.\nMarie CHAMBAUD is the half-sister of Alexandra CHAMBAUD.\nAndre Pierre CHAMBAUD, Male.\nJaqueline CHAMBAUD, Female.\nAndre Pierre CHAMBAUD is the husband of Jaqueline CHAMBAUD.\nJaqueline CHAMBAUD is the wife of Andre Pierre CHAMBAUD.\nFrederique CHAMBAUD is the sister of Mark CHAMBAUD.\n,Frederique CHAMBAUD is the sister of Geraldine CHAMBAUD.\n\nMark CHAMBAUD is the brother of Frederique CHAMBAUD.\n,Mark CHAMBAUD is the brother of Geraldine CHAMBAUD.\n\nGeraldine CHAMBAUD is the sister of Frederique CHAMBAUD.\n,Geraldine CHAMBAUD is the sister of Mark CHAMBAUD.\n\nJaqueline CHAMBAUD is the mother of Frederique CHAMBAUD.\nJaqueline CHAMBAUD is the mother of Mark CHAMBAUD.\nJaqueline CHAMBAUD is the mother of Geraldine CHAMBAUD.\nFrederique CHAMBAUD, Female.\nMark CHAMBAUD, Male.\nGeraldine CHAMBAUD, Female.\nAndre Pierre CHAMBAUD is the father of Frederique CHAMBAUD.\nAndre Pierre CHAMBAUD is the father of Mark CHAMBAUD.\nAndre Pierre CHAMBAUD is the father of Geraldine CHAMBAUD.\nJean-Claude X and Frederique CHAMBAUD are divorced.\nCorto CAILLAT is the brother of Saul CAILLAT.\n\nSaul CAILLAT is the brother of Corto CAILLAT.\n\nMarie CHAMBAUD is the mother of Corto CAILLAT.\nMarie CHAMBAUD is the mother of Saul CAILLAT.\nCorto CAILLAT, Male.\nSaul CAILLAT, Male.\nSimon CAILLAT is the father of Corto CAILLAT.\nSimon CAILLAT is the father of Saul CAILLAT.\nSimon CAILLAT, Male.\nMarie CHAMBAUD, Female, Born 1990 in Aix-en-Provence, FRANCE.\nSimon CAILLAT is the husband of Marie CHAMBAUD.\nMarie CHAMBAUD is the wife of Simon CAILLAT.\nLeon SEMEAC, Male, Born 1896.\nMarie-louise SEMEAC, Female, Born 1899, Dead 1975, Maiden name: BEAUDUN).\nLeon SEMEAC is the husband of Marie-louise SEMEAC.\nMarie-louise SEMEAC is the wife of Leon SEMEAC.\nSimone TORRES is the sister of Suzanne MAURIES.\n\nSuzanne MAURIES is the sister of Simone TORRES.\n\nLeon SEMEAC is the father of Simone TORRES.\nLeon SEMEAC is the father of Suzanne MAURIES.\nSimone TORRES, Female.\nSuzanne MAURIES, Female, Born 1925, Maiden name: SEMEAC.\nMarie-louise SEMEAC is the mother of Simone TORRES.\nMarie-louise SEMEAC is the mother of Suzanne MAURIES.\nJean-max MAURIES, Male.\nSuzanne MAURIES, Female, Born 1925, Maiden name: SEMEAC.\nJean-max MAURIES is the husband of Suzanne MAURIES.\nSuzanne MAURIES is the wife of Jean-max MAURIES.\nMathieu MAURIES is the brother of Pascal MAURIES.\n\nPascal MAURIES is the brother of Mathieu MAURIES.\n\nJean-max MAURIES is the father of Mathieu MAURIES.\nJean-max MAURIES is the father of Pascal MAURIES.\nMathieu MAURIES, Male, Born 1960.\nPascal MAURIES, Male, Born 1962.\nSuzanne MAURIES is the mother of Mathieu MAURIES.\nSuzanne MAURIES is the mother of Pascal MAURIES.\nJules SEMEAC, Male, Born 1867.\nHelene SEMEAC, Female, Born 1876, Maiden name: CONSTANT.\nJules SEMEAC is the husband of Helene SEMEAC.\nHelene SEMEAC is the wife of Jules SEMEAC.\nJules SEMEAC is the father of Leon SEMEAC.\nLeon SEMEAC, Male, Born 1896.\nHelene SEMEAC is the mother of Leon SEMEAC.\nMathieu SEMEAC, Male, Born 1830.\nAnastasie SEMEAC, Female, Born 1843, Maiden name: VERGIER.\nMathieu SEMEAC is the husband of Anastasie SEMEAC.\nAnastasie SEMEAC is the wife of Mathieu SEMEAC.\nCyprien VERGIER is the father of Anastasie SEMEAC.\nAnastasie SEMEAC, Female, Born 1843, Maiden name: VERGIER.\nPierre SEMEAC, Male, born estimated 1809.\nJeanne-Marie SANDARNAUD, Female, born estimated 1809.\nPierre SEMEAC is the husband of Jeanne-Marie SANDARNAUD.\nJeanne-Marie SANDARNAUD is the wife of Pierre SEMEAC.\nPierre SEMEAC is the father of Mathieu SEMEAC.\nMathieu SEMEAC, Male, Born 1830.\nJeanne-Marie SANDARNAUD is the mother of Mathieu SEMEAC.\nEugene CONSTANT, Male, Born 1829.\nAnnette CONSTANT, Female, Maiden name: JAQUIER.\nEugene CONSTANT is the husband of Annette CONSTANT.\nAnnette CONSTANT is the wife of Eugene CONSTANT.\nEugene CONSTANT is the father of Helene SEMEAC.\nHelene SEMEAC, Female, Born 1876, Maiden name: CONSTANT.\nAnnette CONSTANT is the mother of Helene SEMEAC.\nMarcian MAURIES,  (Male).\nJeanne MAURIES,  (Female, Maiden name: TECHOUYERE).\nMarcian MAURIES is the husband of Jeanne MAURIES.\nJeanne MAURIES is the wife of Marcian MAURIES.\nMarcian MAURIES is the father of Jean-max MAURIES.\nJean-max MAURIES, Male.\nJeanne MAURIES is the mother of Jean-max MAURIES.\nPierre BEAUDUN is the father of D??sir?? BEAUDUN.\nD??sir?? BEAUDUN, Male, Born 1867, Dead 1941.\nMarie-louise SEMEAC is the sister of Mireille JAUME.\n,Marie-louise SEMEAC is the sister of Desir?? BEAUDUN the second.\n\nMireille JAUME is the sister of Marie-louise SEMEAC.\n,Mireille JAUME is the sister of Desir?? BEAUDUN the second.\n\nDesir?? BEAUDUN the second is the brother of Marie-louise SEMEAC.\n,Desir?? BEAUDUN the second is the brother of Mireille JAUME.\n\nD??sir?? BEAUDUN is the father of Marie-louise SEMEAC.\nD??sir?? BEAUDUN is the father of Mireille JAUME.\nD??sir?? BEAUDUN is the father of Desir?? BEAUDUN the second.\nMarie-louise SEMEAC, Female, Born 1899, Dead 1975, Maiden name: BEAUDUN).\nMireille JAUME,   (Maiden name: BEAUDUN), Female, Born 1896, Dead 1987.\nDesir?? BEAUDUN the second, Male, Born 1893, Dead 1936.\nCharles Sammuel CHAPUIS, Male, Born 1841, Dead 1922.\nEmilie Sophie CHAPUIS, Maiden name: ROUBY, Female, Born 1840, Dead 1928.\nCharles Sammuel CHAPUIS is the husband of Emilie Sophie CHAPUIS.\nEmilie Sophie CHAPUIS is the wife of Charles Sammuel CHAPUIS.\nEmilie BEAUDUN is the mother of Marie-louise SEMEAC.\nMarie-louise SEMEAC, Female, Born 1899, Dead 1975, Maiden name: BEAUDUN).\nEmilie BEAUDUN is the sister of Madeleine Marie EYGUESIER.\n,Emilie BEAUDUN is the sister of Benjamine Zoe CHAPUIS.\n,Emilie BEAUDUN is the sister of Charles Emile CHAPUIS.\n\nMadeleine Marie EYGUESIER is the sister of Emilie BEAUDUN.\n,Madeleine Marie EYGUESIER is the sister of Benjamine Zoe CHAPUIS.\n,Madeleine Marie EYGUESIER is the sister of Charles Emile CHAPUIS.\n\nBenjamine Zoe CHAPUIS is the sister of Emilie BEAUDUN.\n,Benjamine Zoe CHAPUIS is the sister of Madeleine Marie EYGUESIER.\n,Benjamine Zoe CHAPUIS is the sister of Charles Emile CHAPUIS.\n\nCharles Emile CHAPUIS is the brother of Emilie BEAUDUN.\n,Charles Emile CHAPUIS is the brother of Madeleine Marie EYGUESIER.\n,Charles Emile CHAPUIS is the brother of Benjamine Zoe CHAPUIS.\n\nCharles Sammuel CHAPUIS is the father of Emilie BEAUDUN.\nCharles Sammuel CHAPUIS is the father of Madeleine Marie EYGUESIER.\nCharles Sammuel CHAPUIS is the father of Benjamine Zoe CHAPUIS.\nCharles Sammuel CHAPUIS is the father of Charles Emile CHAPUIS.\nEmilie BEAUDUN, Maiden name: CHAPUIS, Female, Born 1870, Dead 1904.\nMadeleine Marie EYGUESIER, Female, maiden name: CHAPUIS, (Born 1876, Dead 1960).\nBenjamine Zoe CHAPUIS, Female, (Born 1884, Dead 1975).\nCharles Emile CHAPUIS, Male (Born 1863, Dead 1895).\nEmilie Sophie CHAPUIS is the mother of Emilie BEAUDUN.\nEmilie Sophie CHAPUIS is the mother of Madeleine Marie EYGUESIER.\nEmilie Sophie CHAPUIS is the mother of Benjamine Zoe CHAPUIS.\nEmilie Sophie CHAPUIS is the mother of Charles Emile CHAPUIS.\nAntoine Marie FILIPPI, Male.\nBenjamine Zoe CHAPUIS, Female, (Born 1884, Dead 1975).\nAntoine Marie FILIPPI is the husband of Benjamine Zoe CHAPUIS.\nBenjamine Zoe CHAPUIS is the wife of Antoine Marie FILIPPI.\nUnknown, Male.\nFernande ??milienne Louise FILIPPI, Female, Born 1911, Dead 1975.\nUnknown is the husband of Fernande ??milienne Louise FILIPPI.\nFernande ??milienne Louise FILIPPI is the wife of Unknown.\nBenjamine Zoe CHAPUIS is the mother of Fernande ??milienne Louise FILIPPI.\nFernande ??milienne Louise FILIPPI, Female, Born 1911, Dead 1975.\nFernande ??milienne Louise FILIPPI is the mother of Antonine Adrienne COSTE.\nAntonine Adrienne COSTE, Maiden name: FILIPPI, Female.\nAntoine Marius Baptistin FILIPPI is the father of Fernande ??milienne Louise FILIPPI.\nRoger COSTE, Male.\nAntonine Adrienne COSTE, Maiden name: FILIPPI, Female.\nRoger COSTE is the husband of Antonine Adrienne COSTE.\nAntonine Adrienne COSTE is the wife of Roger COSTE.\nClaude Monnier, Male.\nDannie Monnier, Female, (Maiden name: COSTE).\nClaude Monnier is the husband of Dannie Monnier.\nDannie Monnier is the wife of Claude Monnier.\nDannie Monnier is the sister of Andr?? COSTE.\n\nAndr?? COSTE is the brother of Dannie Monnier.\n\nRoger COSTE is the father of Dannie Monnier.\nRoger COSTE is the father of Andr?? COSTE.\nDannie Monnier, Female, (Maiden name: COSTE).\nAndr?? COSTE, Male, AKA DOUDOU.\nAntonine Adrienne COSTE is the mother of Dannie Monnier.\nAntonine Adrienne COSTE is the mother of Andr?? COSTE.\nMadeleine Marie EYGUESIER is the mother of Emily ROCHE.\nEmily ROCHE, Maiden name: EYGUESIER, Female, Born 1904, Dead 1991.\nAdrien Hippolyte Joachim EYGUESIER is the father of Emily ROCHE.\nAdrien Hippolyte Joachim EYGUESIER, Male, Born 1875, Dead 1953.\nMadeleine Marie EYGUESIER, Female, maiden name: CHAPUIS, (Born 1876, Dead 1960).\nAdrien Hippolyte Joachim EYGUESIER is the husband of Madeleine Marie EYGUESIER.\nMadeleine Marie EYGUESIER is the wife of Adrien Hippolyte Joachim EYGUESIER.\nDannie Monnier is the mother of Cecile Mathieu.\nCecile Mathieu, Female, (Maiden name: Monnier).\nClaude Monnier is the father of Cecile Mathieu.\nJean Baptiste Henri ROCHE, (born 1898, dead 1942).\nEmily ROCHE, Maiden name: EYGUESIER, Female, Born 1904, Dead 1991.\nJean Baptiste Henri ROCHE is the husband of Emily ROCHE.\nEmily ROCHE is the wife of Jean Baptiste Henri ROCHE.\nEmily ROCHE is the mother of Albert ROCHE.\nAlbert ROCHE, (AKA B??BERT), Male.\nJean Baptiste Henri ROCHE is the father of Albert ROCHE.\nAlbert ROCHE, (AKA B??BERT), Male.\nJosette ROCHE, Maiden name: RIPPOL, Female.\nAlbert ROCHE is the husband of Josette ROCHE.\nJosette ROCHE is the wife of Albert ROCHE.\nAlbert ROCHE is the father of Jean-Michel ROCHE.\nJean-Michel ROCHE, Male, Born 1966.\nJosette ROCHE is the mother of Jean-Michel ROCHE.\nEtienne ROCHE is the brother of Chloe ROCHE.\n\nChloe ROCHE is the sister of Etienne ROCHE.\n\nJean-Michel ROCHE is the father of Etienne ROCHE.\nJean-Michel ROCHE is the father of Chloe ROCHE.\nEtienne ROCHE, Male.\nChloe ROCHE, Female.\nIsabel ROCHE is the mother of Etienne ROCHE.\nIsabel ROCHE is the mother of Chloe ROCHE.\nPierre JAUME, Male, Born 1888, Dead 1939.\nMireille JAUME,   (Maiden name: BEAUDUN), Female, Born 1896, Dead 1987.\nPierre JAUME is the husband of Mireille JAUME.\nMireille JAUME is the wife of Pierre JAUME.\nRen?? JAUME is the brother of Pierre JAUME the second (AKA P??P??).\n,Ren?? JAUME is the brother of Jean JAUME.\n\nPierre JAUME the second (AKA P??P??) is the brother of Ren?? JAUME.\n,Pierre JAUME the second (AKA P??P??) is the brother of Jean JAUME.\n\nJean JAUME is the brother of Ren?? JAUME.\n,Jean JAUME is the brother of Pierre JAUME the second (AKA P??P??).\n\nPierre JAUME is the father of Ren?? JAUME.\nPierre JAUME is the father of Pierre JAUME the second (AKA P??P??).\nPierre JAUME is the father of Jean JAUME.\nRen?? JAUME, Male.\nPierre JAUME the second (AKA P??P??), Male.\nJean JAUME, Male.\nMireille JAUME is the mother of Ren?? JAUME.\nMireille JAUME is the mother of Pierre JAUME the second (AKA P??P??).\nMireille JAUME is the mother of Jean JAUME.\nPierre JAUME the second (AKA P??P??), Male.\nAndr??e JAUME, Maiden name: FAURE, Female.\nPierre JAUME the second (AKA P??P??) is the husband of Andr??e JAUME.\nAndr??e JAUME is the wife of Pierre JAUME the second (AKA P??P??).\nMarie-Ren??e JAUME is the sister of Bernard JAUME.\n\nBernard JAUME is the brother of Marie-Ren??e JAUME.\n\nPierre JAUME the second (AKA P??P??) is the father of Marie-Ren??e JAUME.\nPierre JAUME the second (AKA P??P??) is the father of Bernard JAUME.\nMarie-Ren??e JAUME, Female.\nBernard JAUME, Male.\nAndr??e JAUME is the mother of Marie-Ren??e JAUME.\nAndr??e JAUME is the mother of Bernard JAUME.\nJean JAUME, Male.\nArlette JAUME, Female.\nJean JAUME is the husband of Arlette JAUME.\nArlette JAUME is the wife of Jean JAUME.\nPierre JAUME the third is the brother of NANOU JAUME.\n\nNANOU JAUME is the sister of Pierre JAUME the third.\n\nJean JAUME is the father of Pierre JAUME the third.\nJean JAUME is the father of NANOU JAUME.\nPierre JAUME the third, AKA PONPON, Male.\nNANOU JAUME, Female.\nArlette JAUME is the mother of Pierre JAUME the third.\nArlette JAUME is the mother of NANOU JAUME.\nPierre JAUME the third, AKA PONPON, Male.\nLucette JAUME, Female.\nPierre JAUME the third is the husband of Lucette JAUME.\nLucette JAUME is the wife of Pierre JAUME the third.\nJULIEN JAUME is the brother of FLORIAN JAUME.\n\nFLORIAN JAUME is the brother of JULIEN JAUME.\n\nPierre JAUME the third is the father of JULIEN JAUME.\nPierre JAUME the third is the father of FLORIAN JAUME.\nJULIEN JAUME, Male.\nFLORIAN JAUME, Male.\nLucette JAUME is the mother of JULIEN JAUME.\nLucette JAUME is the mother of FLORIAN JAUME.\r\n</topic_context>\r\n\r\n";
            var question = "<question>\r\nwhat is the family relationship between Frederic TORRES, Jean-Michel ROCHE and Dannie Monnier?(answer in french, draw a markdown diagram).\r\n</question>\r\n\r\n\r\n";

            Regex quickFilter = new Regex(@"(openai\/gpt-5.6-luna)|(google\/gemini-3.1-flash-lite)");

            foreach (var model in GenericAI.GetModels(quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.SkillTopicQuestion(skill, topic, question, language: "English", model: model.Id);
                Assert.True(result.Text.ToLower().Contains("frederic torres") || result.Text.ToLower().Contains("frédéric torres"));
                Assert.Contains("jean-michel roche", result.Text.ToLower());
                Assert.Contains("dannie monnier", result.Text.ToLower());
                Assert.Contains("charles sammuel chapuis", result.Text.ToLower());

                Assert.True(result.Text.ToLower().Contains("emilie sophie chapuis") || 
                            result.Text.ToLower().Contains("émilie sophie chapuis"));
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateTitle_GenericAI_OpenRouterModels()
        {
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateTitle(text: GlycemicReseachText, language: "English", model: model);
                HttpBase.Trace($"[GENERATE-TITLE] Duration: {result.Duration:00.00}, Model: {model}, Text: {result.Title}", this);
                var cost = client.Completions.LastUsage.ComputeCost();
                Assert.True(cost > 0, $"Cost should be greater than 0 for model {model}");
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateTitle_GenericAI_InterfaceFor_OpenAI_Google_Anthrophic_OpenRouterDeepSeek()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateTitle(text: GlycemicReseachText, language: "English", model: model.Id);
                HttpBase.Trace($"[GENERATE-TITLE] Duration: {result.Duration:0.00}, Model: {model.Id}, Text: {result.Title}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_OpenRouterModels()
        {
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage: "French", model: model);
                HttpBase.Trace($"[TRANSLATE] Duration: {result.Duration:00.00}, Model: {model}, destLanguage: {result.TranslatedText}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_OpenRouterModels()
        {
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model);
                Assert.NotNull(result.Text);
                HttpBase.Trace($"[GENERATE-BULLETPOINT] Duration: {result.Duration:00.00}, Model: {model}, Text: {result.Text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage: "French", model: model);
                HttpBase.Trace($"[TRANSLATE] Model: {model}, Duration: {result.Duration:0.0}, SourceText: {result.SourceText}, destLanguage: {result.TranslatedText}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model.Id);
                HttpBase.Trace($"[GENERATE-BULLETPOINT] Model: {model}, Duration: {result.Duration:0.0}, Text: {result.Text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_IsPassedTheWrongApiKey()
        {
            var model = "gpt-5-nano";
            var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
            var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model);
            Assert.Null(result.Text);
        }

        const string CSharpJsonDotNetQuestion = @"
When using C# and the newtonsoft library, what is the name of the attribute to serialize an enum as a string?
";
        [Fact()]
        [TestBeforeAfter]
        public void Conversation_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.Conversation(text: CSharpJsonDotNetQuestion, model: model.Id);
                Assert.Contains("[JsonConverter(typeof(StringEnumConverter))]", result.Response);
                HttpBase.Trace($"[CONVERSATION] Model: {model.Id}, Duration: {result.Duration:0.0}, Response: {result.Response}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void DetermineTheTypeOfPhrase_InFrench()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model => // _quickFilter
            {
                AIPromptCache.Instance.Clear();
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

                Assert.Equal(GenericAICompletions.PhraseType.Order, client.Completions.DetermineTheTypeOfPhrase("ajouter un element a faire avec the titre suivant", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Paint the sky?", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("quelle est la couleur du ciel?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Analyse, en tant que médecin, les problèmes de santé de Karin et définissez un diagnostic.", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Analyser, en tant que médecin, les problèmes de santé de Karin et définissez un diagnostic.", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Analysez, en tant que médecin, les problèmes de santé de Karin et définissez un diagnostic.", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("recommande, en tant que médecin, les problèmes de santé de Karin et définissez un diagnostic.", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Quelle est ma priorité principale ?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Listez les médecins qui ont diagnostiqué Karen", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Liste les médecins qui ont diagnostiqué Karen", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Lister les médecins qui ont diagnostiqué Karen", model: model.Id));


                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Recherche ce sur quoi Joe travaille aujourd'hui", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Parle-moi du Docteur StrangeLove", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("Le ciel est bleu", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("La terre est basse", model: model.Id));

            });
        }
        [Fact()]
        [TestBeforeAfter]
        public void DetermineTheTypeOfPhrase()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model => // _quickFilter
            {
                AIPromptCache.Instance.Clear();
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

                Assert.Equal(GenericAICompletions.PhraseType.Order, client.Completions.DetermineTheTypeOfPhrase("Add a to-do item with the following title", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Paint the sky?", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is the color of the sky?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Analyse as a Medical Doctor, Karin health issue and issue a diagnostic.", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Recommend as a Medical Doctor, Karin health issue and issue a diagnostic.", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is my highest priority?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("List the doctors whom diagnosticated Karen", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Research what Joe is working on today", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Tell me about Doctor StrangeLove", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("The sky is blue", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("The ground is low", model: model.Id));

            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void Classifier_YesNo_QuestionType()
        {
            AIPromptCache.Instance.Clear();
            var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

            var (yes, _, usage) = client.Completions.CreateClassifier(
                 state: "Phrase: What is the color of the sky?",
                 instructions: "Is this a question?",
                 new GenericAICompletions.ClassifierCriteria
                 {
                     ["false"] = "It is not a question.",
                     ["true"] = "It is a question."
                 }
             );

            Assert.True(yes, "The classifier should return true for this input.");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Classifier_YesNo_ElectricityQuestion()
        {
            AIPromptCache.Instance.Clear();
            var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

            var (yes, _, usage) = client.Completions.CreateClassifier(
                 state: "In electricity, we have high and low, or VCC or GROUND.",
                 instructions: "is Ground low?",
                 new GenericAICompletions.ClassifierCriteria
                 {
                     ["false"] = "Ground is high",
                     ["true"] = "Ground is low"
                 }
             );

            Assert.True(yes, "The classifier should return true for this input.");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Classifier_Choice()
        {
            AIPromptCache.Instance.Clear();
            var client = new GenericAI(); 

            var (_, choice, usage) = client.Completions.CreateClassifier(
                 state: "My running shoes arrived in the wrong size. Can I swap them for a size 10?",
                 instructions: "Which team should handle this?",
                 new GenericAICompletions.ClassifierCriteria
                 {
                    ["returns"] = "Exchanges, wrong or damaged items",
                    ["shipping"] = "Delivery status, delays, lost packages",
                    ["billing"] = "Charges, invoices, payment problems"
                 },
                 type: GenericAICompletions.ClassifierType.choice
             );
            Assert.Equal("returns", choice);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Jev_vs_GeminiFlash__Classifier_Choice_DeterminePhraseType_Performance()
        {
            AIPromptCache.Instance.Clear();
            var client = new GenericAI();
            var phrases = DS.List(
                "Add a to-do item with the following title",
                "Paint the sky?",
                "What is the color of the sky?",
                "Analyse as a Medical Doctor, Karin health issue and issue a diagnostic.",
                "Recommend as a Medical Doctor, Karin health issue and issue a diagnostic.",
                "What is my highest priority?",
                "List the doctors whom diagnosticated Karen",
                "Research what Joe is working on today",
                "Tell me about Doctor StrangeLove",
                "The sky is blue",
                "The ground is low"
            );

            var sw = Stopwatch.StartNew();
            phrases.ForEach(phrase =>
            {
                var phraseType = client.Completions.DetermineTheTypeOfPhraseClassifier(phrase, noneAIOptimization: false);
            });
            sw.Stop();
            Trace($"[PERFORMANCE] Classifier_Choice_DeterminePhraseType_Performance: Duration: {sw.ElapsedMilliseconds/1000f:0.000} ms for {phrases.Count} phrases", this);

            sw = Stopwatch.StartNew();
            phrases.ForEach(phrase =>
            {
                var phraseType = client.Completions.DetermineTheTypeOfPhrase(phrase, DefaultModelToUse, noneAIOptimization: false);
            });
            sw.Stop();
            Trace($"[PERFORMANCE] Classifier_Choice_DeterminePhraseType_Performance: Duration: {sw.ElapsedMilliseconds/1000f:0.000} ms for {phrases.Count} phrases", this);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Classifier_Choice_DeterminePhraseType()
        {
            AIPromptCache.Instance.Clear();
            var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

            Assert.Equal(GenericAICompletions.PhraseType.Order, client.Completions.DetermineTheTypeOfPhraseClassifier("Add a to-do item with the following title"));

            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("Paint the sky?"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("What is the color of the sky?"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("Analyse as a Medical Doctor, Karin health issue and issue a diagnostic."));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("Recommend as a Medical Doctor, Karin health issue and issue a diagnostic."));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("What is my highest priority?"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("List the doctors whom diagnosticated Karen"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("Research what Joe is working on today"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhraseClassifier("Tell me about Doctor StrangeLove"));

            Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhraseClassifier("The sky is blue"));
            Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhraseClassifier("The ground is low"));
        }

        [Fact()]
        [TestBeforeAfter]
        public void RePhraseQuestionIntoAffirmation()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model => //
            {
                try
                {
                    var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
                    var answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my highest priority?", model: model.Id);
                    Assert.Contains("Your highest priority is __SOMETHING__", answer);
                    var j = answer.ToJSON();

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do?", model: model.Id);
                    Assert.Contains("Your next task to do is __SOMETHING__", answer);

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do with the highest priority?", model: model.Id);
                    Assert.Contains("Your next task to do with the highest priority is __SOMETHING__", answer);

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("When is my next meeting?", model: model.Id);
                    Assert.True(
                        answer.Contains("Your next meeting is at __SOMETHING__") ||
                        answer.Contains("Your next meeting is __SOMETHING__")
                        );

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("With whom is my next meeting?", model: model.Id);
                    Assert.Contains("Your next meeting is with __SOMETHING__", answer);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void FixPhrase()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
                var fixedPhrase = client.Completions.FixPhrase("Your to-do number one in the personal section is  Taxes 2025", "English", model: model.Id);
                //Assert.Contains("Your next task to do is __SOMETHING__", fixedPhrase);
                fixedPhrase = client.Completions.FixPhrase("Your highest priority to-do in the personal section is  Create and sign a Will and Trust", "English", model: model.Id);
                fixedPhrase = client.Completions.FixPhrase("What you need to do about your car is  RAV4 Car oil change", "English", model: model.Id);
            });
        }


        const string notes1 = @"
on January 15th, 2026, I had a meeting with John Smith about the new Salesforce integration project in Paris.
The meeting was at 10 AM and it lasted for 1 hour.
I need to prepare a presentation for the next meeting on July 20th, 2026
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_1()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI();
                var medataDictionary = client.Completions.ExtractMetaDataFromNotes(notes1, model: model.Id).MetaData;
                Assert.Equal("John Smith", medataDictionary["people"].First());
                Assert.Equal("Paris", medataDictionary["locations"].First());
                Assert.Equal("2026-01-15", medataDictionary["dates_mentioned"].First());
                Assert.Equal("Salesforce integration", medataDictionary["topics"].First());
                Assert.Equal("task", medataDictionary["type"].First());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void ExtracatMetaData_1()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI();
                var keywords = client.Completions.ExtractKeywordFromNotes(notes1, model: model.Id);
                Assert.True(keywords.Any());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_2()
        {
            var notes2 = @"
On March 3rd, 2026, I had an extended strategy session with 
Sarah Mitchell, 
David Chen, and the newly onboarded project lead, Rebecca Torres, 

regarding the long-overdue overhaul of our legacy CRM platform and 
its proposed integration with both the Salesforce Enterprise suite and 
the third-party analytics tool, DataBridge Pro. 

The meeting, originally scheduled for 9:00 AM in Conference Room B, 
was pushed back by forty-five minutes due to a last-minute conflict with 
David's call with the Singapore office, 

ultimately running well past its allotted two-hour window and wrapping up closer to 12:30 PM. 

During the session, we reviewed the preliminary scoping document that 
Rebecca had circulated the previous 
Thursday, 
flagged several unresolved dependencies around the legacy data migration, 
and agreed that the engineering team would need at least three additional weeks to complete 
their technical audit before any development work could begin. 

Following up on action items, 
I need to revise the project timeline and 
budget estimates in collaboration with the finance liaison, 
Mark Huang, and submit a consolidated report to 
the VP of Operations no later than April 11th, 2026. 

Additionally, 
Sarah has requested that I prepare a detailed risk assessment and a stakeholder presentation, 
both of which are due before our next cross-functional review meeting, 
currently penciled in for May 7th, 2026 at 2:00 PM, 
with a follow-up executive briefing tentatively set for the week of June 22nd, 2026.";

            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")
                var metaData = client.Completions.ExtractMetaDataFromNotes(notes2, model: model.Id);
                ///////////Assert.True(metaData.Keywords.Any());

                var medataDictionary = metaData.MetaData;
                Assert.True(medataDictionary["people"].Any());
                Assert.True(medataDictionary["dates_mentioned"].Any());
                Assert.True(medataDictionary["action_items"].Any());
                Assert.True(medataDictionary["topics"].Any());
                Assert.Equal("task", medataDictionary["type"].First());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void ConvertPdfToMarkdown()
        {
            var client = new GenericAI();
            var markDownText = client.Completions.ConvertPdfToMarkdown(@".\TestFiles\car policy.pdf");
            Assert.Contains("9415857", markDownText);
            Assert.Contains("ALICE TORRES", markDownText);
            Assert.Contains("153 HIGHLAND ST APT 3", markDownText);
        }

        [Fact()]
        [TestBeforeAfter]
        public void TextImprovement_GenericAI_WithSkill()
        {
            var text = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), _randomModelCount);

            models.ForEach(model =>
            {
                var client = new GenericAI();
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model,
                                                                systemPrompt: "diagnose the following patient as a spine orthopedic surgeon:",
                                                                skillName: "spine-orthopedic-surgeon",
                                                                skillRootFolder: @"C:\DVT\fAI\src\fAI.Tests\TestFiles\Skills");

                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, ", this);
                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void AnalyzeImage()
        {
            var models = DS.List(
                "moonshotai/kimi-k3","qwen/qwen3.8-max",
                "google/gemini-3.1-flash-lite", 
                "openai/gpt-5.6-luna",
                "anthropic/claude-opus-4.6", 
                "mistralai/mistral-medium-3-5", 
                "x-ai/grok-4.6"
            );

            var modelsRnd = StringUtil.GetRandom(models.Select(m => m).ToList(), _randomModelCount);

            var imageFileName = base.GetTestFile("ManAndBoartInStorm.png");
            modelsRnd.ForEach(model =>
            {
                var client = new GenericAI();
                var (text, title, usage)    = client.Completions.AnalyzeImage(imageFileName, model);
                Assert.True(DS.List( "ship").All(w => text.ToLower().Contains(w)));

                Assert.True(!string.IsNullOrEmpty(title));
                Assert.True(usage.InputTokens > 0);
                Assert.True(usage.OutputTokens > 0);
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void OcrImageFromFile()
        {
            var models = DS.List(
                "google/gemini-3.1-flash-lite", 
                "openai/gpt-5.6-luna",
                "x-ai/grok-4.6",
                "anthropic/claude-opus-4.6",
                "mistralai/mistral-medium-3-5",
                "moonshotai/kimi-k3", 
                "qwen/qwen3.8-max"
            );

            var modelsRnd = StringUtil.GetRandom(models.Select(m => m).ToList(), _randomModelCount);

            var imageFileName = base.GetTestFile("OCR_1.png");

            modelsRnd.ForEach(model =>
            {
                var client = new GenericAI();
                var (text, title, usage) = client.Completions.OcrImageFromFile(imageFileName, model);
                try
                {
                    Assert.True(DS.List("alice", "approved", "insurance", "policy").All(w => text.ToLower().Contains(w)));
                    Assert.True(!string.IsNullOrEmpty(title));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }
    }
}