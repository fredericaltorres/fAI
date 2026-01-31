using DynamicSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;

/*
    THIS CHAINABLE CLASS IS STILL IN DEVELOPMENT
    NOT READY FOR USE
 */

namespace fAI
{
    /*
     NEW LangChain Expression Language!!
     https://www.youtube.com/watch?v=ud7HJ2p3gp0
     */
    public interface IChainable
    {
        string Invoke(string query);
        void Randomize();
    }

    public partial class Chain 
    {
        GPTPrompt _prompt;
        AnthropicErrorCompletionResponse response;
        Stack<string> _invokedStack = new Stack<string>();

        public string InvokedText => string.Join(Environment.NewLine, _invokedStack.ToArray());
        
        public Chain()
        {
        }

        public string NULL => null as string;

        public string Text { get => response.Text; }

        public Chain Invoke(IChainable chainable, object pocoQuery)
        {
            var parameters = DS.Dictionary(pocoQuery);
            if (parameters.ContainsKey("Query")) 
            {
                _invokedStack.Push(
                    chainable.Invoke(
                        parameters["Query"] == null ? null : parameters["Query"].ToString()
                    )
                );
            }
            if (parameters.ContainsKey("Randomize"))
            {
                chainable.Randomize();
            }
            return this;
        }

        public Chain Invoke(GPTPrompt prompt, object pocoAsDictionary)
        {
            this._prompt = prompt;
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

