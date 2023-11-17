echo off

curl.exe https://api.openai.com/v1/audio/speech -H "Authorization: Bearer %OPENAI_API_KEY%" -H "Content-Type: application/json" -d " { ""model"": ""tts-1"", ""input"": ""The quick brown fox jumped over the lazy dog."", ""voice"": ""alloy"" }" --output speech.mp3

curl.exe https://api.openai.com/v1/audio/speech -H "Authorization: Bearer %OPENAI_API_KEY%" -H "Content-Type: application/json" -d " { ""model"": ""tts-1"", ""input"": ""The quick brown fox jumped over the lazy dog."", ""voice"": ""alloy"" }" --output speech.mp3

curl.exe --request POST --url https://api.openai.com/v1/audio/transcriptions --header "Authorization: Bearer %OPENAI_API_KEY%" --header "Content-Type: multipart/form-data" --form file=@C:\DVT\fAI\src\fAI.Tests\TestFiles\TestFile.01.48Khz.mp3 --form model=whisper-1 --form response_format=text

