import { UpdateProgressByClick } from "./update-progress-by-click";

interface IProgressBarProps {
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
      {/* NOSONAR(S1082) */}
      <progress
        data-test="progress-element"
        ref={progressRef}
        onClick={(event) => UpdateProgressByClick(videoRef, event)}
        className="progress"
        value="0"
      >
        <span id="progress-bar"></span>
      </progress>
    </div>
  );
};
