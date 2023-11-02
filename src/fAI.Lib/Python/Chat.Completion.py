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

#  how_to_count_tokens_with_tiktoken https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken

import os
import sys
import openai
openai.api_key      = os.getenv("OPENAI_API_KEY")
openai.organization = os.getenv("OPENAI_ORGANIZATION_ID")


completion = openai.ChatCompletion.create(
  model="gpt-3.5-turbo",
  messages=[
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Hello!"}
  ]
)

print(completion.choices[0].message)
