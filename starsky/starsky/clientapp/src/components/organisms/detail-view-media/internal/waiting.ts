export function Waiting(
  videoRef: React.RefObject<HTMLVideoElement>,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>
) {
  if (!videoRef.current) return;
  if (videoRef.current.networkState === videoRef.current.NETWORK_LOADING) {
    // The user agent is actively trying to download data.
    setIsLoading(true);
  }
}
