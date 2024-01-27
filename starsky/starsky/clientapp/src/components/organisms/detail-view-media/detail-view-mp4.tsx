import React, { memo, useEffect, useRef, useState } from "react";
import { DetailViewContext } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";
import Preloader from "../../atoms/preloader/preloader";
import { GetVideoClassName } from "./shared/get-video-class-name";
import { PlayPause } from "./shared/play-pause";
import { setDefaultEffect } from "./shared/set-default-effect";
import { TimeUpdate } from "./shared/time-update";
import { UpdateProgressByClick } from "./shared/update-progress-by-click";
import { Waiting } from "./shared/waiting";

const DetailViewMp4: React.FunctionComponent = memo(() => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageVideoNotFound = language.text(
    "Deze video is niet gevonden",
    "This video is not found"
  );

  const MessageVideoPlayBackError = language.text(
    "Er is iets mis met het afspelen van deze video. " +
      "Probeer eens via het menu 'Meer' en 'Download'.",
    "There is something wrong with the playback of this video.  Try 'More' and 'Download'. "
  );

  const history = useLocation();

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);
  const [isError, setIsError] = useState("");
  const { state } = React.useContext(DetailViewContext);

  /** update to make useEffect simpler te read */
  const [downloadPhotoApi, setDownloadPhotoApi] = useState(
    new UrlQuery().UrlDownloadPhotoApi(
      new URLPath().encodeURI(
        new URLPath().getFilePath(history.location.search)
      ),
      false
    )
  );

  useEffect(() => {
    setDefaultEffect(
      history.location.search,
      setDownloadPhotoApi,
      videoRef,
      scrubberRef,
      progressRef,
      timeRef
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [new URLPath().getFilePath(history.location.search)]);

  // Check if media is found
  useEffect(() => {
    if (!state?.fileIndexItem?.status) return;
    if (state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing) {
      setIsError(MessageVideoNotFound);
      return;
    }
    setIsError("");
  }, [state, MessageVideoNotFound]);

  const videoRef = useRef<HTMLVideoElement>(null);
  const progressRef = useRef<HTMLProgressElement>(null);
  const scrubberRef = useRef<HTMLSpanElement>(null);
  const timeRef = useRef<HTMLSpanElement>(null);

  const [paused, setPaused] = useState(true);
  const [started, setStarted] = useState(false);

  const videoRefCurrent = videoRef.current;
  useEffect(() => {
    if (!videoRefCurrent) return;
    // when video ends
    videoRefCurrent.addEventListener("ended", setPausedTrue);
    // As the video is playing, update the progress bar
    videoRefCurrent.addEventListener("timeupdate", () =>
      TimeUpdate(videoRef, setIsLoading, progressRef, scrubberRef, timeRef)
    );
    videoRefCurrent.addEventListener("waiting", () =>
      Waiting(videoRef, setIsLoading)
    );

    return () => {
      // Unbind the event listener on clean up
      if (!videoRefCurrent) return;
      videoRefCurrent.removeEventListener("ended", setPausedTrue);
      videoRefCurrent.removeEventListener("timeupdate", () =>
        TimeUpdate(videoRef, setIsLoading, progressRef, scrubberRef, timeRef)
      );
      videoRefCurrent.removeEventListener("waiting", () =>
        Waiting(videoRef, setIsLoading)
      );
    };
  }, [videoRefCurrent]);

  function setPausedTrue() {
    setPaused(true);
  }

  return (
    <>
      {isLoading ? <Preloader isWhite={false} isOverlay={false} /> : ""}
      {isError ? (
        <Notification
          callback={() => setIsError("")}
          type={NotificationType.danger}
        >
          {isError}
        </Notification>
      ) : null}

      {!isError ? (
        <figure
          data-test="video"
          className={GetVideoClassName(paused, started)}
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
            event.key === "Enter" &&
              TimeUpdate(
                videoRef,
                setIsLoading,
                progressRef,
                scrubberRef,
                timeRef
              );
          }}
          onClick={() => {
            PlayPause(
              videoRef,
              setIsError,
              MessageVideoPlayBackError,
              setStarted,
              paused,
              setPaused,
              setIsLoading
            );
            TimeUpdate(
              videoRef,
              setIsLoading,
              progressRef,
              scrubberRef,
              timeRef
            );
          }}
        >
          <video
            playsInline={true}
            ref={videoRef}
            controls={false}
            preload="metadata"
          >
            <track
              kind="captions"
              src={downloadPhotoApi.replace("mp4", "srt")}
            />
            <source src={downloadPhotoApi} type="video/mp4" />
          </video>
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
          </div>
        </figure>
      ) : null}
    </>
  );
});
export default DetailViewMp4;
