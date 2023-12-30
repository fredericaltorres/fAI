# fAI

# Introduction

* AI Experimentation With Different Providers in C#
 * OpenAI
    - Querying GPT
    - Image generation (DALL-E)
    - Embeddings
    - Speech To Text
    - Text To Speech
 * Microsoft Cognitive Services
    - Text To Speech
    - Azure Search Index with Embeddings
 * Deepgram
    - Speech To Text


![Logo ](fAI.1.50.png "Logo")

# Overview
Library fAI available on NuGet.

## Querying OpenAI GPT models

Querying and expecting answer in JSON format.
```csharp
[Fact()]
public void Completion_JsonMode_WorldCup()
{
    var client = new OpenAI();
    var p = new Prompt_GPT_35_Turbo_JsonAnswer
    {
        Messages = new List<GPTMessage>()
        {
            new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant designed to output JSON." },
            new GPTMessage { Role =  MessageRole.user,   Content = "Who won the soccer world cup in 1998?" }
        }
    };
    var response = client.Completions.Create(p);
    Assert.True(response.Success);
    var answer = response.JsonObject["winner"];
    Assert.Equal("France", answer);
}
```

Conversation and data analysis with GPT 4
```csharp
[Fact()]
public void Completion_Chat_QuestionAboutPastSchedule()
{
    var client = new OpenAI();
    var prompt = new Prompt_GPT_4
    {
        Messages = new List<GPTMessage> 
        {
            new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant." },
            new GPTMessage { Role =  MessageRole.user,   Content = $"08/02/2021 15:00 Meeting with Eric." },
            new GPTMessage { Role =  MessageRole.user,   Content = $"09/01/2021 15:00 Meeting with Eric." },
            new GPTMessage { Role =  MessageRole.user,   Content = $"09/10/2021 10:00 Take the dog to the vet." },
            new GPTMessage { Role =  MessageRole.user,   Content = $"09/20/2021 15:00 Meeting with Rick and John" },
        }
    };
    var response = client.Completions.Create(prompt);

    prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "When was the last time I talked with Eric?" });
    response = client.Completions.Create(prompt);
    Assert.Matches(@"Eric.*09\/01\/2021 at 15:00", response.Text);

    prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What do I have to do on 09/10/2021?" });
    response = client.Completions.Create(prompt);
    Assert.Matches(@"dog.*vet.*10:00", response.Text);
}
```


Translation with model gpt-3.5-turbo

```csharp
public string                     Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage);
```

```csharp
const string ReferenceEnglishSentence = "Hello world.";
[Fact()]
public void Translate_EnglishToSpanish()
{
    var client = new OpenAI();
    var translation = client.Completions.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.Spanish);
    Assert.Equal("'Hola mundo.'", translation);
}
```
Others helper to execute translations. Translating dictionary or list of string can do 2 things
1. Batch translating more than one text with 1 call to GPT.
2. For a dictionary the `key` is not translated as part of the answer and can be use to map the translation
with the original texts.

```csharp
public Dictionary<string, string> Translate(Dictionary<string, string> inputDictionary, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage);
public List<string>               Translate(List<string> strings, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage);
```


## Querying OpenAI Embeddings API

```csharp
 public class OpenAIEmbeddingsTests
 {
     [Fact()]
     public void Embeddings_Create()
     {
         var input = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
         var client = new OpenAI();
         var r = client.Embeddings.Create(input);

         // r.Data[0].Embedding contains the list of float 

         Assert.Equal("embedding", r.Data[0].Object);
         Assert.Equal(r.Data[0].EmbeddingMaxValue, r.Data[0].Embedding.Count);
         Assert.Equal(37, r.Usage.PromptTokens);
         Assert.Equal(37, r.Usage.TotalTokens);
     }
 }
```

## Querying OpenAI DALL-E. Image Generation

```csharp
[Fact()]
public void Image_Generate()
{
    var prompt = @"Generate an image inspired by Victor Hugo's classic novel, 'Les Misérables'. 
The image should depict three characters, each with distinct characteristics. 
The first is an older, physically strong man with a scarred face, wearing threadbare clothes, indicative of a hard life — this represents Jean Valjean. 
The second is a young woman radiating innocence and kindness; she wears modest clothes and has beautiful shining eyes — this is Cosette. 
The third is a stern-looking middle-aged man in a gentleman's attire and hat,  representing law and order — representative of Javert. 
Their expressions should reflect the nuances of the complex relationships 
they share in the story.
";
    var client = new OpenAI();
    var r = client.Image.Generate(prompt, size :  OpenAIImageSize._1792x1024);
    var pngFileNames = r.DownloadImage();
    Assert.True(pngFileNames.Count == 1);
    Assert.True(File.Exists(pngFileNames[0]));
}
```

<img src=".\Image_Generate.example.png" alt="Image_Generate.example.png" width="448"/>


# Third parties API Key
API keys can be hard coded in code or read automatically from environment variable. 

|Environment Variable Names|
|----------------------------|
|MICROSOFT_AZURE_SEARCH_KEY|
|MICROSOFT_COGNITIVE_SERVICES_KEY|
|MICROSOFT_COGNITIVE_SERVICES_REGION|
|OPENAI_API_KEY|
|OPENAI_ORGANIZATION_ID|
|OPENAI_LOG_FILE|
|DEEPGRAM_API_KEY|
