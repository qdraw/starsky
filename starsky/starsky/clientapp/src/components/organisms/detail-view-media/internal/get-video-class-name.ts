// ignore: Provide multiple methods instead of using "isPaused" to determine which action to take.
export function GetVideoClassName(isPaused: boolean, isStarted: boolean): string {
  if (isPaused) {
    if (isStarted) {
      return "video play";
    } else {
      return "video first";
    }
  } else {
    return "video pause";
  }
}
