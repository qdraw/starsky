import useGlobalSettings from "../../../../hooks/use-global-settings";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { PlayPause } from "./play-pause";
import { ProgressBar } from "./progress-bar";

export interface IControlsProps {
  scrubberRef: React.RefObject<HTMLSpanElement>;
  progressRef: React.RefObject<HTMLProgressElement>;
  videoRef: React.RefObject<HTMLVideoElement>;
  paused: boolean;
  setPaused: React.Dispatch<React.SetStateAction<boolean>>;
  setIsError: React.Dispatch<React.SetStateAction<string>>;
  setStarted: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  timeRef: React.RefObject<HTMLSpanElement>;
}

export const Controls: React.FunctionComponent<IControlsProps> = ({
  scrubberRef,
  progressRef,
  videoRef,
  paused,
  setPaused,
  setIsError,
  setStarted,
  setIsLoading,
  timeRef
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageVideoPlayBackError = language.key(localization.MessageVideoPlayBackError);

  return (
    <div className="controls">
      <button
        className={paused ? "play" : "pause"}
        onClick={() =>
          PlayPause(
            videoRef,
            setIsError,
            MessageVideoPlayBackError,
            setStarted,
            paused,
            setPaused,
            setIsLoading
          )
        }
        onKeyDown={(event) => {
          event.key === "Enter" &&
            PlayPause(
              videoRef,
              setIsError,
              MessageVideoPlayBackError,
              setStarted,
              paused,
              setPaused,
              setIsLoading
            );
        }}
        type="button"
      >
        <span className="icon"></span>
        {paused ? "Play" : "Pause"}
      </button>
      <span ref={timeRef} data-test="video-time" className="time"></span>
      <ProgressBar scrubberRef={scrubberRef} progressRef={progressRef} videoRef={videoRef} />
    </div>
  );
};
