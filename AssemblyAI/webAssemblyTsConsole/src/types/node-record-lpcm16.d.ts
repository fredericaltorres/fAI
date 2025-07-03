declare module 'node-record-lpcm16' {
  
  interface Options {
    sampleRate?: number;
    channels?: number;
    threshold?: number;
    endOnSilence?: boolean;
    recorder?: string;
    device?: string;
    audioType?: string;
  }

  interface Recorder {
    record(options?: Options): {
      stream(): NodeJS.Readable;
      stop(): void;
    };
  }

  const recorder: Recorder;
  export default recorder;
}
