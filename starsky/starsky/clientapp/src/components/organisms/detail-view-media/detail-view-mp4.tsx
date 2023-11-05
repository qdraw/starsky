import React, { memo, useEffect, useRef, useState } from "react";
import { DetailViewContext } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { secondsToHours } from "../../../shared/date";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";
import Preloader from "../../atoms/preloader/preloader";

function GetVideoClass(isPaused: boolean, isStarted: boolean): string {
  if (isPaused) {
    if (isStarted) {
      return "video play";
    } else {
      return "video first";
    }
  } else {
    return "video pause";
  }
}

// eslint-disable-next-line react/display-name
const DetailViewMp4: React.FunctionComponent = memo(() => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageVideoPlayBackError = language.text(
    "Er is iets mis met het afspelen van deze video. " +
      "Probeer eens via het menu 'Meer' en 'Download'.",
    "There is something wrong with the playback of this video.  Try 'More' and 'Download'. "
  );
  const MessageVideoNotFound = language.text(
    "Deze video is niet gevonden",
    "This video is not found"
  );

  const history = useLocation();

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);
  const [isError, setIsError] = useState("");
  const { state } = React.useContext(DetailViewContext);

  /** update to make useEffect simpler te read */
  const [downloadApi, setDownloadPhotoApi] = useState(
    new UrlQuery().UrlDownloadPhotoApi(
      new URLPath().encodeURI(
        new URLPath().getFilePath(history.location.search)
      ),
      false
    )
  );

  useEffect(() => {
    const downloadApiLocal = new UrlQuery().UrlDownloadPhotoApi(
      new URLPath().encodeURI(
        new URLPath().getFilePath(history.location.search)
      ),
      false,
      true
    );
    setDownloadPhotoApi(downloadApiLocal);

    if (
      !videoRef.current ||
      !scrubberRef.current ||
      !progressRef.current ||
      !timeRef.current
    ) {
      return;
    }

    videoRef.current.setAttribute("src", downloadApiLocal);
    videoRef.current.load();

    // after a location change
    progressRef.current.removeAttribute("max");
    videoRef.current.currentTime = 0;
    scrubberRef.current.style.left = "0%";
    progressRef.current.value = 0;
    timeRef.current.innerHTML = "";
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

  const [isPaused, setPaused] = useState(true);
  const [isStarted, setStarted] = useState(false);

  const videoRefCurrent = videoRef.current;
  useEffect(() => {
    if (!videoRefCurrent) return;
    // when video ends
    videoRefCurrent.addEventListener("ended", setPausedTrue);
    // As the video is playing, update the progress bar
    videoRefCurrent.addEventListener("timeupdate", timeUpdate);
    videoRefCurrent.addEventListener("waiting", waiting);

    return () => {
      // Unbind the event listener on clean up
      if (!videoRefCurrent) return;
      videoRefCurrent.removeEventListener("ended", setPausedTrue);
      videoRefCurrent.removeEventListener("timeupdate", timeUpdate);
      videoRefCurrent.removeEventListener("waiting", waiting);
    };
  }, [videoRefCurrent]);

  function setPausedTrue() {
    setPaused(true);
  }

  function playPause() {
    if (!videoRef.current) return;
    if (videoRef.current.play === undefined) {
      setIsError(MessageVideoPlayBackError);
      return;
    }

    setStarted(true);

    if (isPaused) {
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

  function timeUpdate() {
    if (
      !videoRef.current ||
      !progressRef.current ||
      !scrubberRef.current ||
      !timeRef.current
    )
      return;

    // For mobile browsers, ensure that the progress element's max attribute is set
    if (
      !progressRef.current.getAttribute("max") &&
      !isNaN(videoRef.current.duration)
    ) {
      progressRef.current.setAttribute(
        "max",
        videoRef.current.duration.toString()
      );
    }

    progressRef.current.value = videoRef.current.currentTime;

    // scrubber bol
    const srubberPercentage =
      (progressRef.current.value / videoRef.current.duration) * 100;
    scrubberRef.current.style.left = srubberPercentage + "%";

    // time
    timeRef.current.innerHTML = `${secondsToHours(
      videoRef.current.currentTime
    )} / ${secondsToHours(videoRef.current.duration)}`;

    // to disable the loading is slow
    setIsLoading(false);
  }

  function getMousePosition(event: React.MouseEvent | MouseEvent) {
    const target = event.target as HTMLProgressElement;
    if (!target.offsetLeft) return 0.1;
    return (
      (event.pageX -
        (target.offsetLeft + (target.offsetParent as HTMLElement).offsetLeft)) /
      target.offsetWidth
    );
  }

  function updateProgressByClick(event: React.MouseEvent) {
    if (!videoRef.current || !event.target) return;
    const mousePosition = getMousePosition(event);

    const result = isNaN(mousePosition)
      ? mousePosition * videoRef.current.duration
      : 0;
    console.log("result", result);

    videoRef.current.currentTime = result;
  }

  function waiting() {
    if (!videoRef.current) return;
    if (videoRef.current.networkState === videoRef.current.NETWORK_LOADING) {
      // The user agent is actively trying to download data.
      setIsLoading(true);
    }
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
          className={GetVideoClass(isPaused, isStarted)}
          onKeyDown={(event) => {
            event.key === "Enter" && playPause();
            event.key === "Enter" && timeUpdate();
          }}
          onClick={() => {
            playPause();
            timeUpdate();
          }}
        >
          <video
            playsInline={true}
            ref={videoRef}
            controls={false}
            preload="metadata"
          >
            <source src={downloadApi} type="video/mp4" />
          </video>
          <div className="controls">
            <button
              className={isPaused ? "play" : "pause"}
              onClick={playPause}
              onKeyDown={(event) => {
                event.key === "Enter" && playPause();
              }}
              type="button"
            >
              <span className="icon"></span>
              {isPaused ? "Play" : "Pause"}
            </button>
            <span ref={timeRef} data-test="video-time" className="time"></span>
            <div className="progress">
              <span ref={scrubberRef} className="scrubber"></span>
              <progress
                ref={progressRef}
                onClick={updateProgressByClick}
                className="progress"
                value="0"
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
