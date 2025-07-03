// Install the required packages by executing the command "npm install assemblyai stream node-record-lpcm16"
// https://sourceforge.net/projects/sox/

import dotenv from "dotenv";
import { Readable } from 'stream'
import { AssemblyAI } from 'assemblyai'
import recorder from 'node-record-lpcm16'

dotenv.config();

function getApiKey() {
    const apiKey = process.env.WEBASSEMBLY_API_KEY;
    if (!apiKey) 
        throw new Error("WEBASSEMBLY_API_KEY is not set");
    return apiKey;
}
function trace(message: string) {
    console.log(message);
}

trace("webAssemblyTsConsole");
trace(`WEBASSEMBLY_API_KEY: ${getApiKey()}`);

const run = async () => {

  const client = new AssemblyAI({ apiKey: getApiKey() });

  const transcriber = client.streaming.transcriber({
    sampleRate: 16_000,
    formatTurns: true
  });

  transcriber.on("open", ({ id }) => {
    console.log(`Session opened with ID:${id}`)
  });

  transcriber.on("error", (error) => {
    console.error("Error:", error);
  });

  transcriber.on("close", (code, reason) =>
    console.log("Session closed:", code, reason),
  );

  transcriber.on("turn", (turn) => {
    if (!turn.transcript) {
      return;
    }

    console.log("Turn:", turn.transcript);
  });

  try {
    console.log("Connecting to streaming transcript service");

    await transcriber.connect();

    console.log("Starting recording");
    
    const recording = recorder.record({
      channels: 1,
      sampleRate: 16_000,
      audioType: "wav", // Linear PCM
    });

    Readable.toWeb(recording.stream()).pipeTo(transcriber.stream());

    // Stop recording and close connection using Ctrl-C.

    process.on("SIGINT", async function () {
      console.log();
      console.log("Stopping recording");
      recording.stop();

      console.log("Closing streaming transcript connection");
      await transcriber.close();

      process.exit();
    });
  } catch (error) {
    console.error(error);
  }
};

run();
