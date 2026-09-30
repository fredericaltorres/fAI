## About GGUF 

GGUF is a binary file format for storing large language models so they can be loaded and run efficiently, especially on local hardware like laptops, desktops, and consumer GPUs. It was introduced in August 2023 by the llama.cpp project (Georgi Gerganov's C/C++ inference engine) as the successor to the older GGML format. The name is usually expanded as "GPT-Generated Unified Format" or "GGML Universal File," though the project has never been strict about it.

A GGUF file bundles everything needed to run a model into one file. It has a header, then a set of key-value metadata, then the tensor data. The metadata holds the model architecture (Llama, Mistral, Qwen, and so on), hyperparameters like context length and layer count, the tokenizer and its vocabulary, special tokens, and often the chat template. The tensor data holds the actual weights. Because all of this lives in one file, you don't need separate config or tokenizer files the way you do with a typical Hugging Face checkpoint.

The format was designed with a few goals in mind. It is memory-mappable, so the weights can be loaded from disk quickly without copying everything into RAM first. It is extensible, so new metadata fields can be added without breaking older readers, which was a big problem with GGML. It also has first-class support for quantization, which is why it became popular.

Quantization is the main reason people use GGUF. Model weights are normally stored as 16-bit floats, but GGUF files usually store them at lower precision, from 8 bits down to about 2 bits per weight. You'll see this in file names with labels like Q8_0, Q5_K_M, Q4_K_M, or IQ2_XS. Lower-bit versions are much smaller and faster but lose some quality. For example, a 7B-parameter model that takes about 14 GB at 16-bit might be around 4 GB at Q4_K_M, small enough to run on an ordinary laptop. Q4_K_M and Q5_K_M are common choices because they keep most of the model's quality.

GGUF is the native format for llama.cpp and for tools built on it or compatible with it, including Ollama, LM Studio, GPT4All, KoboldCpp, and Jan. Hugging Face hosts thousands of GGUF models, often uploaded by community members who convert and quantize popular open-weight models. You'd typically pick GGUF when you want to run a model locally on a CPU, an Apple Silicon Mac, or a modest GPU. For training, fine-tuning, or high-throughput GPU serving, formats like safetensors (used with PyTorch, vLLM, and similar tools) are more common.



## About the console app

his is a .NET 8 console app that reads a GGUF file's header and prints it. Put both files in one folder and run dotnet run -- path/to/model.gguf. Add --tensors to list every tensor, --full to show long strings like chat templates in full, or --preview N to change how many array items are shown.

It prints the GGUF version, counts, alignment and where the data section starts. It then lists every metadata entry: architecture, context length, tokenizer settings, and so on. Last comes a tensor summary with the total parameter count and a breakdown by quantization type. Large arrays such as a 150k-token vocabulary show only the first few items, and the rest is skipped on disk rather than loaded. It only reads the header, so even a 70 GB file finishes almost instantly.

The file is treated as untrusted input, in line with OWASP guidance. Every length and count is checked against limits and the remaining file size before anything is allocated or skipped. Nested arrays have a depth cap. Text from the file is escaped before printing so it can't inject terminal control sequences.

I couldn't compile or run this here because there's no .NET SDK in my environment, so please tell me if you get any build errors and I'll fix them.