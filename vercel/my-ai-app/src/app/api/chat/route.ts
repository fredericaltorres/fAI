/*
  https://ai-sdk.dev/docs/getting-started/nextjs-app-router
  https://github.com/cosdensolutions/code/blob/master/videos/long/vercel-ai-sdk-tutorial/src/app/api/chat/route.ts
*/

import { openai } from "@ai-sdk/openai";
import { streamText, tool } from 'ai';
import { z } from 'zod';

// Allow streaming responses up to 30 seconds
export const maxDuration = 30;

export async function POST(req: Request) {
  const { messages } = await req.json();

  console.log(`Backend calling OpenAI API`);

  const result = streamText({
    model: openai("gpt-4o-mini"),
    messages,
    system: `
    Frederic Torres was born in December 1964 in Aix-en-Provence, France. 
    He is a software engineer and a founder of a company called 'fLogViewer'. 
    `,
    onFinish: (message) => {
      console.log(`Backend received message: ${message.text}`);
    },
    // tools: {
    //   weather: tool({
    //     description: 'Get the weather in a location (fahrenheit)',
    //     parameters: z.object({
    //       location: z.string().describe('The location to get the weather for'),
    //     }),
    //     execute: async ({ location }) => {
    //       const temperature = Math.round(Math.random() * (90 - 32) + 32);
    //       return {
    //         location,
    //         temperature,
    //       };
    //     },
    //   }),
    // }
  });

  return result.toDataStreamResponse();
}