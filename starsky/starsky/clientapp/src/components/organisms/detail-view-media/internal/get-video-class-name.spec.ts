import { GetVideoClassName } from "./get-video-class-name";

describe("GetVideoClassName function", () => {
  it('should return "video play" when paused and started', () => {
    const isPaused = true;
    const isStarted = true;

    const result = GetVideoClassName(isPaused, isStarted);

    expect(result).toBe("video play");
  });

  it('should return "video first" when paused and not started', () => {
    const isPaused = true;
    const isStarted = false;

    const result = GetVideoClassName(isPaused, isStarted);

    expect(result).toBe("video first");
  });

  it('should return "video pause" when not paused', () => {
    const isPaused = false;
    const isStarted = true;

    const result = GetVideoClassName(isPaused, isStarted);

    expect(result).toBe("video pause");
  });
});
