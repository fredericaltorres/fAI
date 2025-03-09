using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace fAI.Util
{
    public class TextSplitter
    {
        /// <summary>
        /// Splits a text document into paragraphs suitable for vector database storage
        /// </summary>
        /// <param name="text">The input text to be split</param>
        /// <param name="minLength">Minimum paragraph length in characters (default: 50)</param>
        /// <param name="maxLength">Maximum paragraph length in characters (default: 1000)</param>
        /// <param name="wordOverlapCount">Number of words to overlap between chunks (default: 0)</param>
        /// <returns>A list of text chunks ready to be vectorized</returns>
        public static List<string> SplitIntoParagraphs(
            string text, 
            int minLength = 128, 
            int maxLength = 4096, 
            int wordOverlapCount = 32)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            // Clean up the text - normalize whitespace
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Split by paragraph markers
            string[] rawParagraphs = Regex.Split(text, @"(?<=\.|\?|\!)\s+");

            List<string> resultChunks = new List<string>();
            StringBuilder currentChunk = new StringBuilder();

            foreach (string paragraph in rawParagraphs)
            {
                // Skip empty paragraphs
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;

                // If adding this paragraph would exceed max length, 
                // store the current chunk and start a new one
                if (currentChunk.Length > 0 && currentChunk.Length + paragraph.Length > maxLength)
                {
                    string chunk = currentChunk.ToString().Trim();
                    if (chunk.Length >= minLength)
                        resultChunks.Add(chunk);

                    // Handle overlap if specified
                    if (wordOverlapCount > 0 && chunk.Length > 0)
                    {
                        string[] words = chunk.Split(' ');
                        if (words.Length > wordOverlapCount)
                        {
                            currentChunk.Clear();
                            for (int i = Math.Max(0, words.Length - wordOverlapCount); i < words.Length; i++)
                            {
                                currentChunk.Append(words[i] + " ");
                                if(words[i].EndsWith(".") && i < words.Length-1)
                                    currentChunk.Clear();
                            }
                        }
                        else currentChunk.Clear();
                    }
                    else currentChunk.Clear();
                }
                currentChunk.Append(paragraph + " ");
            }
            
            if (currentChunk.Length >= minLength) // Add the last chunk if it's not empty and meets minimum length
                resultChunks.Add(currentChunk.ToString().Trim());

            return resultChunks;
        }

    }
}
