# Ollama

## how to use embedding model with ollama localy making an http call

```
ollama pull nomic-embed-text

curl.exe http://localhost:11434/api/embeddings -H "Content-Type: application/json" -d "{ ""model"": ""nomic-embed-text"", ""prompt"": ""Your text to embed here"" }"
curl.exe http://localhost:11434/api/embeddings -H "Content-Type: application/json" -d "{ ""model"": ""qwen3-embedding:8b"", ""prompt"": ""Your text to embed here"" }"




ollama list
ollama run nomic-embed-text
ollama run qwen3-embedding:8b
ollama run qwen3-embedding:0.6b
ollama run qwen3-embedding:8b
ollama rm qwen3-embedding:8b
ollama ps
ollama stop nomic-embed-text
ollama launch  

ollama show nomic-embed-text-v2-moe:latest --modelfile 
"C:\Users\FredericTorres\.ollama\models\blobs\sha256-a5db3381f2e514d3490a3a31fe70eb1a65e95016c85c6c2c23223b810806594f"

```


## Commmands

```
 serve        Start Ollama
  create       Create a model
  show         Show information for a model
  run          Run a model
  stop         Stop a running model
  pull         Pull a model from a registry
  push         Push a model to a registry
  signin       Sign in to ollama.com
  signout      Sign out from ollama.com
  list         List models
  ps           List running models
  cp           Copy a model
  rm           Remove a model
  launch       Launch the Ollama menu or an integration
  help         Help about any command
```

## fAI.Beetles.All

https://ollama.com/search?c=embedding

```
./fAI.Beetles.All.exe ollama-nomic
./fAI.Beetles.All.exe ollama-nomic-v2
./fAI.Beetles.All.exe openai
```

 