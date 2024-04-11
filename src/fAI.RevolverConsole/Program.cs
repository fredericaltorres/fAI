using DynamicSugar;
using fAI.Pinecone.Model;
using fAI.VectorDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI.RevolverConsole
{
    internal class Program
    {
        const string BeatlesRevolverIndexName = "beetles-revolver";

        public static void CreateIndex()
        {
            var client = new PineconeDB();
            if (!client.ExistsIndex(BeatlesRevolverIndexName))
            {
                var index = client.CreateIndex(BeatlesRevolverIndexName);
                var e = EmbeddingRecord.FromJsonFile(@".\Revolver.json");

                foreach (var er in e)
                {
                    var pv = new PineconeVector { id = er.Id, values = er.Embedding, metadata = DS.Dictionary(new { text = er.Text }) };
                    var r1 = client.UpsertVectors(index, new List<PineconeVector> { pv });
                }
            }
        }

        static void Write(string message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = originalColor;
        }

        static void WriteLine(string message, ConsoleColor color)
        {
            Write(message, color);
            Console.WriteLine();
        }

        static void WriteQuestion(string message) => WriteLine(message, ConsoleColor.Yellow);
        static void WriteInformation(string message)  => WriteLine(message, ConsoleColor.White);
        static void WriteAnswer(string message) => WriteLine(message, ConsoleColor.Green);

        static void Main(string[] args)
        {
            Console.Clear();
            CreateIndex();
            var message = "Enter search criteria about the lyrics of the Beatles' Album Revolver ?";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var client = new PineconeDB();
            var index = client.GetIndex(BeatlesRevolverIndexName);
            var minimumScore = 0.75f;
            var topK = 3;

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;

                if (!criteria.IsNullOrEmpty())
                {
                    var response = client.SimilaritySearch(index, criteria, topK, minimumScore: minimumScore);
                    foreach (var r in response.matches)
                        WriteAnswer($"Id: {r.id}, {r.score}");
                    Console.WriteLine($"");
                }

                WriteQuestion(message);
            }
        }
    }
}
/*
 
var inMemoryEmbeddingRecords = EmbeddingRecord.FromJsonFile(@".\Revolver.json");


var inMemoryResponse = SimilaritySearchEngine.SimilaritySearch(SimilaritySearchEngine.ToVector(criteria), inMemoryEmbeddingRecords, topK, minimumScore);
foreach (var r in inMemoryResponse)
WriteAnswer($"Id: {r.Id}, {r.Score}");
Console.WriteLine($"");
 


question to chat gpt 4
using openai embeddings, when 2  sentences are similar, how do I understand why?

Understanding "why" two sentences are deemed similar by OpenAI embeddings is more nuanced and challenging compared to simply computing a similarity score. Embeddings are high-dimensional vectors that represent the semantic meaning of text, but they don't directly encode interpretable reasons for similarity. The embeddings capture nuanced patterns in language from the data they were trained on, but these patterns are encoded in a complex, high-dimensional space that isn't straightforward to interpret.

However, there are approaches you can take to gain insights into what might be driving the similarity between two sentences according to their embeddings:

1. Feature Attribution Methods:
Gradient-based Approaches: For neural network models, gradient-based feature attribution methods (like Integrated Gradients, GradientSHAP) can sometimes be used to identify which parts of the input text most influenced the model's embeddings. These methods, however, require access to the model's internals and gradients, which might not be directly available via the OpenAI API.
2. Semantic Analysis:
Manual Analysis: Sometimes, a manual examination of the sentences can provide insights. Look for shared keywords, synonyms, related concepts, or similar structures.
Part-of-Speech Tagging and Dependency Parsing: Use NLP tools to analyze the grammatical structure of the sentences. This can help identify similar usage of nouns, verbs, adjectives, etc., which might contribute to the semantic similarity.
3. Visualization:
Dimensionality Reduction: Techniques like PCA (Principal Component Analysis) or t-SNE can reduce the dimensionality of the embeddings to 2D or 3D. Visualizing these can sometimes provide insights into how different sentences cluster together semantically.
4. Contrastive Analysis:
Modify and Compare: You can modify one of the sentences incrementally (e.g., changing words, removing phrases) and observe how these modifications affect the similarity score. This might give clues as to which parts of the sentence are most critical for their perceived similarity.
5. Analogies and Relationships:
Exploring Relationships: By examining sentences that are known to be related in specific ways (e.g., through analogies, opposites, cause-effect) and comparing their embeddings, you might develop intuitions about how certain types of semantic relationships are represented.
6. Consult the Model's Documentation:
OpenAI's documentation and research publications can sometimes provide insights into how their models are trained, what kinds of linguistic features they pay attention to, and how they generally understand similarity.
Limitations and Ethical Considerations
It's important to note that while these methods can provide insights, they may not always give a clear or complete understanding of why two sentences are deemed similar by their embeddings. The interpretability of machine learning models, especially those based on deep learning, is an active area of research and comes with its own set of challenges and limitations.

Furthermore, when attempting to interpret model outputs, it's crucial to be aware of the potential for models to replicate or amplify biases present in their training data. This awareness should guide the analysis and the application of insights derived from model outputs
 */