import { GetMousePosition } from "./get-mouse-position";

export function UpdateProgressByClick(
  videoRef: React.RefObject<HTMLVideoElement>,
  event?: React.MouseEvent
) {
  if (!videoRef.current || !event?.target) return;

  const mousePosition = GetMousePosition(event);

  const result = !isNaN(mousePosition)
    ? mousePosition * videoRef.current.duration
    : 0;

  videoRef.current.currentTime = result;
}
