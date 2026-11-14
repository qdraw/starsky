import { GetMousePosition } from "./get-mouse-position";

export function UpdateProgressByClick(
  videoRef: React.RefObject<HTMLMediaElement | null>,
  event?: React.MouseEvent
) {
  if (!videoRef.current || !event?.target) return;

  const mousePosition = GetMousePosition(event);

  const result = Number.isNaN(mousePosition) ? 0 : mousePosition * videoRef.current.duration;

  videoRef.current.currentTime = result;
}
