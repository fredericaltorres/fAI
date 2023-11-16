echo off

curl.exe https://api.openai.com/v1/audio/speech -H "Authorization: Bearer %OPENAI_API_KEY%" -H "Content-Type: application/json" -d " { ""model"": ""tts-1"", ""input"": ""The quick brown fox jumped over the lazy dog."", ""voice"": ""alloy"" }" --output speech.mp3
