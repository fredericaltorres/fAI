/*

https://ai-sdk.dev
    Vercel AI SDK in React (https://www.youtube.com/watch?v=y4IMq43KvRw)
    Next.js App Router Quickstart (https://ai-sdk.dev/docs/getting-started/nextjs-app-router)

    Video Source Code:
    https://github.com/cosdensolutions/code/blob/master/videos/long/vercel-ai-sdk-tutorial/src/app/page.tsx


    Discussion:
    list the books of baudelaire?
    give me a short bio of the author.
    List other poets friends of the author.
    Did the author meet Victor Hugo?

*/
"use client";

import { useEffect, useState } from "react";
import { Message, useChat } from "@ai-sdk/react";

export default function Home() {
  const [messages2, setMessages2] = useState([ { id: "1", role: "user", content: "HI, how are you?" } ]);

console.log("messages2", messages2);

  const { messages, input, handleInputChange, handleSubmit } = useChat({
    api: "/api/chat",
    initialMessages: messages2 as Message[],
    onFinish: (message) => {
      setMessages2([...messages2, { id: message.id, role: message.role , content: message.content }]);
    },
  });

  return (
    <div className="flex flex-col w-full max-w-md py-24 mx-auto stretch">
      {messages.map((message) => (
        <div key={message.id} className="whitespace-pre-wrap">
          {message.role === "user" ? "User: " : "AI: "}
          {message.parts.map((part, i) => {
            switch (part.type) {
              case "text":
                return <div key={`${message.id}-${i}`}>{part.text}</div>;
            }
          })}
        </div>
      ))}

      <form onSubmit={handleSubmit}>
        <input
          className="fixed dark:bg-zinc-900 bottom-0 w-full max-w-md p-2 mb-8 border border-zinc-300 dark:border-zinc-800 rounded shadow-xl"
          value={input}
          placeholder="Ask me something..."
          onChange={handleInputChange}
        />
      </form>
    </div>
  );
}