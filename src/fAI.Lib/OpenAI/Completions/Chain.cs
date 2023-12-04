using DynamicSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace fAI
{
    /*
     NEW LangChain Expression Language!!
     https://www.youtube.com/watch?v=ud7HJ2p3gp0

     */
    public partial class Chain 
    {
        GPTPrompt _prompt;
        CompletionResponse response;

        public Chain()
        {
        }
        public string Text { get => response.Text; }

        public Chain Prompt(GPTPrompt prompt)
        {
            this._prompt = prompt;
            return this;
        }
        public Chain Invoke(object pocoAsDictionary)
        {
            var parameters = DS.Dictionary(pocoAsDictionary);
            if (this._prompt == null) throw new Exception("Prompt is null");
            var client = new OpenAI();
            this._prompt.ProcessPrompt(parameters);
            response = client.Completions.Create(this._prompt);
            if (response.Success)
            {
            }
            else
            {
                // response.Exception.Message;
            }
            return this;
        }

    }
}

