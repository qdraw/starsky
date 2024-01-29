export function PlayPause(
  videoRef: React.RefObject<HTMLVideoElement>,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  MessageVideoPlayBackError: string,
  setStarted: React.Dispatch<React.SetStateAction<boolean>>,
  paused: boolean,
  setPaused: React.Dispatch<React.SetStateAction<boolean>>,
  setIsLoading: (value: React.SetStateAction<boolean>) => void
) {
  console.log(videoRef);

  if (!videoRef.current) return;
  if (videoRef.current.play === undefined) {
    setIsError(MessageVideoPlayBackError);
    return;
  }

  setStarted(true);

  if (paused) {
    const promise = videoRef.current.play();

    promise?.catch(() => {
      setIsError(MessageVideoPlayBackError);
    });

    setPaused(false);
    setIsLoading(true);
    return;
  }
  setPaused(true);
  videoRef.current.pause();
}
