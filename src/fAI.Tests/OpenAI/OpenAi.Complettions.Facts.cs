using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using DynamicSugar;
using static System.Net.Mime.MediaTypeNames;
using static fAI.OpenAICompletionsEx;

namespace fAI.Tests
{


    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAiCompletionsFacts : OpenAiCompletionsBase
    {

        public OpenAiCompletionsFacts()
        {
            OpenAI.TraceOn = true;
        }
            
        const string KingOfFrances = @"
            ""Hugh Capet"" was king of France from 987 to 996.
            ""Robert II"" was king of France from 996 to 1031.
            ""Henry I"" was king of France from 1031 to 1060.
            ""Philip I"" was king of France from 1060 to 1108.
            ""Louis VI"" was king of France from 1108 to 1137.
            ""Louis VII"" was king of France from 1137 to 1180.
            ""Philip II"" was king of France from 1180 to 1223.
            ""Louis VIII"" was king of France from 1223 to 1226.
            ""Louis IX (Saint Louis)"" was king of France from 1226 to 1270.
            ""Philip III"" was king of France from 1270 to 1285.
            ""Philip IV"" was king of France from 1285 to 1314.
            ""Louis X"" was king of France from 1314 to 1316.
            ""John I"" was king of France from 1316 to 1316.
            ""Philip V"" was king of France from 1316 to 1322.
            ""Charles IV"" was king of France from 1322 to 1328.
            ""Philip VI (Valois Branch)"" was king of France from 1328 to 1350.
            ""John II"" was king of France from 1350 to 1364.
            ""Charles V"" was king of France from 1364 to 1380.
            ""Charles VI"" was king of France from 1380 to 1422.
            ""Charles VII"" was king of France from 1422 to 1461.
            ""Louis XI"" was king of France from 1461 to 1483.
            ""Charles VIII"" was king of France from 1483 to 1498.
            ""Louis XII"" was king of France from 1498 to 1515.
            ""Francis I"" was king of France from 1515 to 1547.
            ""Henry II"" was king of France from 1547 to 1559.
            ""Francis II"" was king of France from 1559 to 1560.
            ""Charles IX"" was king of France from 1560 to 1574.
            ""Henry III"" was king of France from 1574 to 1589.
            ""Henry IV (Bourbon Branch)"" was king of France from 1589 to 1610.
            ""Louis XIII"" was king of France from 1610 to 1643.
            ""Louis XIV"" was king of France from 1643 to 1715.
            ""Louis XV"" was king of France from 1715 to 1774.
            ""Louis XVI"" was king of France from 1774 to 1792.";

        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_Answered()
        {
            var client = new OpenAI();
            var question = "Who was king of france in 1032?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Assert.Equal("Henry I", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_AnswerNotFound()
        {
            var client = new OpenAI();
            var question = "Who was king of france in 2016?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Assert.Equal("I could not find an answer.", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_HowManyYearsWasLouisXIVking()
        {
            var client = new OpenAI();
            var question = @"How many years was ""Louis XIV"" king?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Assert.Equal("72", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateOneMultiChoiceQuestionAboutText()
        {
            var questionCount = 1;
            var dbFact = new DBFact();
            dbFact.AddFacts(KingOfFrances, randomizeOrder: true);

            var client = new OpenAI();
            var questions = client.CompletionsEx.GenerateMultiChoiceQuestionAboutText(questionCount, dbFact.GetText());
            Assert.Equal(questionCount, questions.Count);

            var question = questions[0];
            Assert.True(question.Text.Length > 0);
            Assert.True(question.Answers.Count > 0);
            Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateThreeMultiChoiceQuestionAboutText()
        {
            var questionCount = 3;
            var dbFact = new DBFact().AddFacts(KingOfFrances, randomizeOrder: true);
            var client = new OpenAI();
            var questions = client.CompletionsEx.GenerateMultiChoiceQuestionAboutText(questionCount, dbFact.GetText());
            Assert.Equal(questionCount, questions.Count);

            foreach (var question in questions)
            {
                Assert.True(question.Text.Length > 0);
                Assert.True(question.Answers.Count > 0);
                Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void MultiChoiceQuestion_FromText_OneAnswer()
        {
            var text = "Who was the king of France from 1285 to 1314?\n\nA) Louis IX\nB) Philip III\nC) Philip IV*\nD) Louis X";
            var q = MultiChoiceQuestion.FromText(text, 1)[0];
            Assert.Equal("Who was the king of France from 1285 to 1314?", q.Text);
            Assert.Equal("A) Louis IX", q.Answers[0]);
            Assert.Equal("B) Philip III", q.Answers[1]);
            Assert.Equal("C) Philip IV", q.Answers[2]);
            Assert.Equal("D) Louis X", q.Answers[3]);
            Assert.Equal(2, q.CorrectAnswerIndex);
        }

        [Fact()]
        [TestBeforeAfter]
        public void MultiChoiceQuestion_FromText_ThreeAnswer()
        {
            var text = @"
1. Who was the king of France from 1285 to 1314?
A. Louis XIII
B. Philip IV*
C. Charles V
D. John II

2. Which king ruled France from 1422 to 1461?
A. Louis IX (Saint Louis)
B. Charles VII*
C. Henry IV (Bourbon Branch)
D. Philip VI (Valois Branch)

3. When did Louis XIV reign as king of France?
A. 1643 to 1715*
B. 1515 to 1547
C. 1285 to 1314
D. 1574 to 1589
";
            var questions = MultiChoiceQuestion.FromText(text, 3);
            Assert.Equal(3, questions.Count);
            var q = questions[0];
            Assert.Equal("Who was the king of France from 1285 to 1314?", q.Text);
            Assert.Equal("A. Louis XIII", q.Answers[0]);
            Assert.Equal("B. Philip IV", q.Answers[1]);
            Assert.Equal("C. Charles V", q.Answers[2]);
            Assert.Equal("D. John II", q.Answers[3]);
            Assert.Equal(1, q.CorrectAnswerIndex);

            q = questions[2];
            Assert.Equal("When did Louis XIV reign as king of France?", q.Text);
            Assert.Equal("A. 1643 to 1715", q.Answers[0]);
            Assert.Equal("B. 1515 to 1547", q.Answers[1]);
            Assert.Equal("C. 1285 to 1314", q.Answers[2]);
            Assert.Equal("D. 1574 to 1589", q.Answers[3]);
            Assert.Equal(0, q.CorrectAnswerIndex);
        }

        /*
        [Fact()]
        [TestBeforeAfter]
        public void GenerateMultiChoiceQuestionAboutText_Chain_DBFact()
        {
            var dbFact = new FactDB();
            dbFact.AddFacts(KingOfFrances);

            IChainable factDB = dbFact;
            var chain = new Chain();
            var client = new OpenAI();
            var prompt = client.Completions.GetPromptForGenerateMultiChoiceQuestionAboutText(string.Join(Environment.NewLine, dbFact.Facts.Values));

            var answer = chain
                            .Invoke(factDB, new { Randomize = true })
                            .Invoke(factDB, new { Query = chain.NULL }) // Get all facts
                            .Invoke(prompt, new {  }) // Context = chain.InvokedText, Question = ""
                            .Text;

            var question = MultiChoiceQuestion.FromText(answer);

            Assert.True(question.Text.Length > 0);
            Assert.True(question.Answers.Count > 0);
            Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);

        }
*/
    }
}




//const string KingOfFrances = @"
//""Hugh Capet"" was king of France from 987 to 996.
//""Robert II"" was king of France from 996 to 1031.
//""Henry I"" was king of France from 1031 to 1060.
//""Philip I"" was king of France from 1060 to 1108.
//""Louis VI"" was king of France from 1108 to 1137.
//""Louis VII"" was king of France from 1137 to 1180.
//""Philip II"" was king of France from 1180 to 1223.
//""Louis VIII"" was king of France from 1223 to 1226.
//""Louis IX (Saint Louis)"" was king of France from 1226 to 1270.
//""Philip III"" was king of France from 1270 to 1285.
//""Philip IV"" was king of France from 1285 to 1314.
//""Louis X"" was king of France from 1314 to 1316.
//""John I"" was king of France from 1316 to 1316.
//""Philip V"" was king of France from 1316 to 1322.
//""Charles IV"" was king of France from 1322 to 1328.
//""Philip VI (Valois Branch)"" was king of France from 1328 to 1350.
//""John II"" was king of France from 1350 to 1364.
//""Charles V"" was king of France from 1364 to 1380.
//""Charles VI"" was king of France from 1380 to 1422.
//""Charles VII"" was king of France from 1422 to 1461.
//""Louis XI"" was king of France from 1461 to 1483.
//""Charles VIII"" was king of France from 1483 to 1498.
//""Louis XII"" was king of France from 1498 to 1515.
//""Francis I"" was king of France from 1515 to 1547.
//""Henry II"" was king of France from 1547 to 1559.
//""Francis II"" was king of France from 1559 to 1560.
//""Charles IX"" was king of France from 1560 to 1574.
//""Henry III"" was king of France from 1574 to 1589.
//""Henry IV (Bourbon Branch)"" was king of France from 1589 to 1610.
//""Louis XIII"" was king of France from 1610 to 1643.
//""Louis XIV"" was king of France from 1643 to 1715.
//""Louis XV"" was king of France from 1715 to 1774.
//""Louis XVI"" was king of France from 1774 to 1792.
//";