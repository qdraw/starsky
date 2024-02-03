import { SecondsToHours } from "../../../../shared/date";

export function TimeUpdate(
  videoRef: React.RefObject<HTMLVideoElement>,
  setIsLoading: (value: React.SetStateAction<boolean>) => void,
  progressRef: React.RefObject<HTMLProgressElement>,
  scrubberRef: React.RefObject<HTMLSpanElement>,
  timeRef: React.RefObject<HTMLSpanElement>
) {
  if (!videoRef.current || !progressRef.current || !scrubberRef.current || !timeRef.current) return;

  // For mobile browsers, ensure that the progress element's max attribute is set
  if (!progressRef.current.getAttribute("max") && !isNaN(videoRef.current.duration)) {
    progressRef.current.setAttribute("max", videoRef.current.duration.toString());
  }

  progressRef.current.value = videoRef.current.currentTime;

  // scrubber bol
  const srubberPercentage = (progressRef.current.value / videoRef.current.duration) * 100;
  scrubberRef.current.style.left = srubberPercentage + "%";

  // time
  timeRef.current.innerHTML = `${SecondsToHours(
    videoRef.current.currentTime
  )} / ${SecondsToHours(videoRef.current.duration)}`;

  // to disable the loading is slow
  setIsLoading(false);
}
