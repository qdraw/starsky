import { UpdateProgressByClick } from "./update-progress-by-click";

export interface IProgressBarProps {
  scrubberRef: React.RefObject<HTMLSpanElement>;
  progressRef: React.RefObject<HTMLProgressElement>;
  videoRef: React.RefObject<HTMLVideoElement>;
}

export const ProgressBar: React.FunctionComponent<IProgressBarProps> = ({
  scrubberRef,
  progressRef,
  videoRef
}) => {
  return (
    <div className="progress">
      <span ref={scrubberRef} className="scrubber"></span>
      <progress
        ref={progressRef}
        onClick={(event) => UpdateProgressByClick(videoRef, event)}
        className="progress"
        value="0"
        onKeyDown={(event) => {
          event.key === "Enter" && UpdateProgressByClick(videoRef);
        }}
      >
        <span id="progress-bar"></span>
      </progress>
    </div>
  );
};
