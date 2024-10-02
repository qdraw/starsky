export function GetVideoClassName(isPaused: boolean, isStarted: boolean): string {
  if (isPaused) {
    return handleIsStarted(isStarted);
  } else {
    return "video pause";
  }
}

function handleIsStarted(isStarted: boolean) {
  if (isStarted) {
    return "video play";
  } else {
    return "video first";
  }
}
