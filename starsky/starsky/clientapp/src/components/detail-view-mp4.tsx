import React, { memo, useEffect, useRef, useState } from "react";
import useLocation from '../hooks/use-location';
import { secondsToHours } from '../shared/date';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import Preloader from './preloader';


const DetailViewMp4: React.FunctionComponent = memo(() => {
  var history = useLocation();

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  /** update to make useEffect simpler te read */
  const [downloadApi, setDownloadPhotoApi] = useState(new UrlQuery().UrlDownloadPhotoApi(
    new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)), false)
  );

  useEffect(() => {
    var downloadApiLocal = new UrlQuery().UrlDownloadPhotoApi(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)), false);
    setDownloadPhotoApi(downloadApiLocal);

    if (!videoRef.current || !scrubberRef.current || !progressRef.current || !timeRef.current) return;
    videoRef.current.setAttribute('src', downloadApiLocal);
    videoRef.current.load();

    // after a location change
    progressRef.current.removeAttribute('max');
    videoRef.current.currentTime = 0;
    scrubberRef.current.style.left = "0%";
    progressRef.current.value = 0;
    timeRef.current.innerHTML = "";

  }, [history.location.search]);

  const videoRef = useRef<HTMLVideoElement>(null);
  const progressRef = useRef<HTMLProgressElement>(null);
  const scrubberRef = useRef<HTMLSpanElement>(null);
  const timeRef = useRef<HTMLSpanElement>(null);

  const [isPaused, setPaused] = useState(true);
  const [isStarted, setStarted] = useState(false);

  var videoRefCurrent = videoRef.current
  useEffect(() => {
    if (!videoRefCurrent) return;
    // when video ends
    videoRefCurrent.addEventListener('ended', setPausedTrue);
    // As the video is playing, update the progress bar
    videoRefCurrent.addEventListener('timeupdate', timeUpdate);
    videoRefCurrent.addEventListener('waiting', waiting);

    return () => {
      // Unbind the event listener on clean up
      if (!videoRefCurrent) return;
      videoRefCurrent.removeEventListener('ended', setPausedTrue);
      videoRefCurrent.removeEventListener('timeupdate', timeUpdate);
      videoRefCurrent.removeEventListener('waiting', waiting);
    };
  }, [videoRefCurrent]);

  function setPausedTrue() {
    setPaused(true);
  }

  function playPause() {
    if (!videoRef.current) return;

    setStarted(true);
    setPaused(!videoRef.current.paused);

    if (videoRef.current.paused) {
      videoRef.current.play();
      setIsLoading(true);
      return;
    }
    videoRef.current.pause();
  }

  function timeUpdate() {
    if (!videoRef.current || !progressRef.current || !scrubberRef.current || !timeRef.current) return;

    // For mobile browsers, ensure that the progress element's max attribute is set
    if (!progressRef.current.getAttribute('max') && !isNaN(videoRef.current.duration)) {
      progressRef.current.setAttribute('max', videoRef.current.duration.toString());
    }

    progressRef.current.value = videoRef.current.currentTime;

    // scrubber bol
    var srubberPercentage = progressRef.current.value / videoRef.current.duration * 100;
    scrubberRef.current.style.left = srubberPercentage + "%";

    // time
    timeRef.current.innerHTML = `${secondsToHours(videoRef.current.currentTime)} / ${secondsToHours(videoRef.current.duration)}`

    // to disable the loading is slow
    setIsLoading(false);
  }

  function getMousePosition(event: React.MouseEvent | MouseEvent) {
    const target = (event.target as HTMLProgressElement);
    return (event.pageX - (target.offsetLeft + (target.offsetParent as HTMLElement).offsetLeft)) / target.offsetWidth;
  }

  function updateProgressByClick(event: React.MouseEvent) {
    if (!videoRef.current || !event.target) return;
    var mousePosition = getMousePosition(event);
    videoRef.current.currentTime = !isNaN(mousePosition) ? mousePosition * videoRef.current.duration : 0;
  }

  function waiting() {
    if (!videoRef.current) return;
    if (videoRef.current.networkState === videoRef.current.NETWORK_LOADING) {
      // The user agent is actively trying to download data.
      setIsLoading(true)
    }
  }

  return (<>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}
    <figure data-test="video" className={isPaused ? isStarted ? "video play" : "video first" : "video pause"} onClick={() => { playPause(); timeUpdate(); }}>
      <video playsInline={true} ref={videoRef} controls={false} preload="metadata">
        <source src={downloadApi} type="video/mp4" />
      </video>
      <div className="controls">
        <button className={isPaused ? "play" : "pause"} onClick={playPause} type="button">{isPaused ? "Play" : "Pause"}</button>
        <span ref={timeRef} className="time"></span>
        <div className="progress">
          <span ref={scrubberRef} className="scrubber"></span>
          <progress ref={progressRef} onClick={updateProgressByClick} className="progress" value="0">
            <span id="progress-bar"></span>
          </progress>
        </div>
      </div>
    </figure>
  </>);
});
export default DetailViewMp4
