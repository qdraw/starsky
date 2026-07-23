import { GetVideoClassName } from "./get-video-class-name";

describe("GetVideoClassName function", () => {
  it.each([
    {
      description: 'return "video play" when paused and started',
      isPaused: true,
      isStarted: true,
      expected: "video play"
    },
    {
      description: 'return "video first" when paused and not started',
      isPaused: true,
      isStarted: false,
      expected: "video first"
    },
    {
      description: 'return "video pause" when not paused',
      isPaused: false,
      isStarted: true,
      expected: "video pause"
    }
  ])("should $description", ({ isPaused, isStarted, expected }) => {
    const result = GetVideoClassName(isPaused, isStarted);

    expect(result).toBe(expected);
  });
});
