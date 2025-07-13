/*

https://ai-sdk.dev
    Vercel AI SDK in React (https://www.youtube.com/watch?v=y4IMq43KvRw)
    Next.js App Router Quickstart (https://ai-sdk.dev/docs/getting-started/nextjs-app-router)

    Video Source Code:
    https://github.com/cosdensolutions/code/blob/master/videos/long/vercel-ai-sdk-tutorial/src/app/page.tsx

*/
"use client";

import { useChat } from "@ai-sdk/react";

export default function Home() {
  const { messages, input, handleInputChange, handleSubmit } = useChat({
    api: "/api/chat",
    initialMessages: [
      { id: "1", role: "user", content: "HI, how are you?" },
    ],
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
          placeholder="Say something..."
          onChange={handleInputChange}
        />
      </form>
    </div>
  );
}