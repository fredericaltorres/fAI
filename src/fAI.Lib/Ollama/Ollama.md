# Ollama


## how to use embedding model with ollama localy making an http call

```
ollama pull nomic-embed-text

curl.exe http://localhost:11434/api/embeddings -H "Content-Type: application/json" -d "{ ""model"": ""nomic-embed-text"", ""prompt"": ""Your text to embed here"" }"

ollama ps
ollama stop nomic-embed-text



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