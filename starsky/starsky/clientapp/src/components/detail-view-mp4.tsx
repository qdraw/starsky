import React, { memo, useEffect, useRef, useState } from "react";
import useLocation from '../hooks/use-location';
import { secondsToHours } from '../shared/date';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';


const DetailViewMp4: React.FunctionComponent = memo(() => {
  var history = useLocation();

  /** update to make useEffect simpler te read */
  const [filePathEncoded, setFilePathEncoded] = useState(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)));
  useEffect(() => {
    setFilePathEncoded(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)))
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

    return () => {
      // Unbind the event listener on clean up
      if (!videoRefCurrent) return;
      videoRefCurrent.removeEventListener('ended', setPausedTrue);
      videoRefCurrent.removeEventListener('timeupdate', timeUpdate);
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
      return;
    }
    videoRef.current.pause();
  }

  function timeUpdate() {
    console.log('timeupdate -->');

    if (!videoRef.current || !progressRef.current || !scrubberRef.current || !timeRef.current) return;

    // For mobile browsers, ensure that the progress element's max attribute is set
    if (!progressRef.current.getAttribute('max')) progressRef.current.setAttribute('max', videoRef.current.duration.toString());
    progressRef.current.value = videoRef.current.currentTime;

    // scrubber bol
    var srubberPercentage = progressRef.current.value / videoRef.current.duration * 100;
    scrubberRef.current.style.left = srubberPercentage + "%";

    // time
    timeRef.current.innerHTML = `${secondsToHours(videoRef.current.currentTime)} / ${secondsToHours(videoRef.current.duration)}`
  }

  function getMousePosition(event: React.MouseEvent | MouseEvent) {
    const target = (event.target as HTMLProgressElement);
    return (event.pageX - (target.offsetLeft + (target.offsetParent as HTMLElement).offsetLeft)) / target.offsetWidth;
  }

  function updateProgressByClick(event: React.MouseEvent) {
    if (!videoRef.current || !progressRef.current || !event.target) return;
    videoRef.current.currentTime = getMousePosition(event) * videoRef.current.duration;
  }


  return (<>
    <figure data-test="video" className={isPaused ? isStarted ? "video play" : "video first" : "video pause"} onClick={() => { playPause(); timeUpdate(); }}>
      <video playsInline={true} ref={videoRef} controls={false} preload="metadata">
        <source src={new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false)} type="video/mp4" />
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
