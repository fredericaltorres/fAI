# Machine configuration
# Install Python 3.10.4 # issue installing openai with newer version. Add python to the system path
# Re-start vsCode, In vs code install extension
#   - Python Lense 
#   - Pip Manager
# Using new and special tab in vsCode from PIP Manager install the following
#   openai
#	    Other useful playsound v 1.2.2 pick version, pydub, zmq, scipy, wavio, google-cloud-texttospeech, openai, google-cloud-speech
# See this video
#   Install Video https://www.youtube.com/watch?v=l9IQsyzFM60
#   Build a Summarizer and Q&A with ChatGPT/GPT-3 APIs - https://www.youtube.com/watch?v=OgSpLwTVyHQ

# C:\Users\ftorres\AppData\Local\Programs\Python\Python312\python.exe "C:\Brainshark\development\platform\Brainshark.Cognitive.Library.UnitTests\LanguageTranslationProvider\Python Samples\ChatGPT.py"

import os
import sys
import openai
openai.api_key      = os.getenv("OPENAI_API_KEY")
openai.organization = os.getenv("OPENAI_ORGANIZATION_ID")
# model = openai.Model.list()
# print(model)

# use gpt2tokenizerfast
def tokenize_gpt2(text):
    from transformers import GPT2TokenizerFast
    tokenizer = GPT2TokenizerFast.from_pretrained('gpt2')
    return tokenizer.encode(text)

def openai_inference_gpt3(prompt, max_tokens):
    r = openai.Completion.create(model = "text-davinci-003", prompt = prompt, max_tokens = max_tokens, temperature = 0)
    return r.choices[0].text

def inference(prompt, max_tokens):
    return openai_inference_gpt3(prompt, max_tokens)

def split_text(tokenized, budget):
    split = []
    for i in range(0, len(tokenized), budget):
        split.append(tokenized[i : i + budget])

    if len(split) > 1 and len(split[-1]) < budget:
        # calculate the remaining budget
        remaining_budget = budget - len(split[-1])
        # pad the last chunk with the remaining budget
        split[-1] = split[-2][-remaining_budget:] + split[-1]
                
    return split    

def detokenize_gpt2(tokenized):
    from transformers import GPT2TokenizerFast
    tokenizer = GPT2TokenizerFast.from_pretrained('gpt2')
    return tokenizer.decode(tokenized)

# https://platform.openai.com/tokenizer
def summarize(text):
    result = tokenize_gpt2(text)
    split = split_text(result, TOKEN_BUDGET)

    # if split has several chunk
    if len(split) > 1:
        # summarize each chunk
        summaries = []
        for i,chunk in enumerate(split):
            chunk_text = detokenize_gpt2(chunk)
            summaries.append(summarize(chunk_text))
            print(i, summaries[-1])
        summaries = " ".join(summaries)
    else:
        summaries = text
    
    prompt = f"{PRE_PROMPT} {summaries} {POST_PROMPT}"
    r = openai_inference_gpt3(prompt, max_tokens = MAX_NEW_TOKENS)
    return r

PRE_PROMPT          = "Summarize the following text: \n===\n"
POST_PROMPT         = "\n===\n Summary:\n"
MAX_TOKENS          = 4000
MAX_NEW_TOKENS      = 500
TOKEN_BUDGET        = MAX_TOKENS - len(tokenize_gpt2(PRE_PROMPT)) - len(tokenize_gpt2(POST_PROMPT)) - MAX_NEW_TOKENS

max_tokens = 25
# commandLineArgument  = sys.argv[1] # arg.0 is the file name

# r = openai.Completion.create(model = "text-davinci-003", prompt = "Say this is a test", max_tokens = max_tokens)
# print(f'text: {r.choices[0].text}, total_tokens:{r.usage.total_tokens}')

# model GPT 3, text-davinci-003 does not know who he is? unlike chatGPT

prompt = "Say this is a test"
prompt = "Q: What is the diameter of the earth?"
prompt = "Q: What is your name?"
prompt = "This is a conversation with Elon Musk. Q: Why have you bought twtter?"
prompt = "This is a conversation with Einstein. Q: Why have you bought twtter?"
prompt = "Hi how are you? A:"

#r = inference(prompt, max_tokens)
#print(f'text: {r}')

bigText = """
Hey there, everyone! I'm Jordan Lee, and I'm super excited to be here with you today because 
I've got somethin to share with you that is going to blow your mind!
 Introducing the all-new "SwiftGadget X" – the gadget of your dreams! This little marvel is not just a device; 
it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. 
Trust me, folks, this isn't your ordinary gadget – this is a game-changer. 
Imagine having the world at your fingertips, with lightning-fast performance, crystal-clear display, and a battery life 
that  seems to go on forever. You won't miss a beat with SwiftGadget X by your side! 
Now, I know what you might be thinking – Jordan, this sounds too good to be true." 
But let me tell you, we've put SwiftGadget X through the wringer. 
We've tested it in extreme conditions, pushed its limits, and it came out on top every single time. 
We believe in this product so much that we're offering an exclusive deal just for you, our online community. 
You won't find this anywhere else, folks. 
It's our way of saying thank you for believing in innovation and excellence. 
So, what are you waiting for? Click that order button below and join the revolution. 
I promise you, once you experience it, you'll wonder how you ever lived without it. 
Thanks for tuning in, and I can't wait to welcome you to the SwiftGadget X family. 
Here's to a smarter, faster, and more exciting tomorrow!
"""

# result = tokenize_gpt2(bigText)
# print(result)
# split = split_text(result, TOKEN_BUDGET)
# print(split)
# print([len(x) for x in split])

print(summarize(bigText))

##summarize(bigText)


###########r = openai.Completion.create(model = "text-davinci-003", prompt = prompt, max_tokens = max_tokens)
#print(f'text: {r.choices[0].text}, total_tokens:{r.usage.total_tokens}')

