import React, { memo, useEffect, useRef, useState } from 'react';
import useIntersection from '../hooks/use-intersection-observer';
import useLocation from '../hooks/use-location';
import { ImageFormat } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import EmptyImage from '../style/images/empty-image.gif';

interface IListImageProps {
  fileHash: string;
  imageFormat?: ImageFormat;
  alt?: string;
}

const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {

  const target = useRef<HTMLDivElement>(null);
  var alt = props.alt ? props.alt : 'afbeelding';

  const [src, setSrc] = useState(props.fileHash);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  // to update the url, and trigger loading if the url is changed
  useEffect(() => {
    setError(false);
    setIsLoading(true);
  }, [props.fileHash]);

  var intersected = useIntersection(target, {
    rootMargin: '250px',
    once: true,
    threshold: 0.3
  });
  // threshold = indicate at what percentage of the target's visibility the callback is executed. (default = 0)

  // to stop loading images after a url change
  var history = useLocation();
  const [historyLocation] = useState(history.location.search);
  useEffect(() => {
    // use ?f only to support details
    // need to refresh
    if (new URLPath().getFilePath(historyLocation) !== new URLPath().getFilePath(history.location.search)
      && isLoading) {
      // data:images are blocked by a strict CSP 
      setSrc(EmptyImage) // 26 bytes
      return;
    }
    setSrc(`/api/thumbnail/${props.fileHash}.jpg?issingleitem=${(localStorage.getItem("issingleitem") !== "false").toString()}`)
  }, [props.fileHash, history.location.search, historyLocation, isLoading]);


  if (props.fileHash === 'null' || props.fileHash === null || !props.fileHash || !props.imageFormat) {
    return (<div ref={target} className="img-box--error" />);
  }

  // for example show gpx, raw and mp4 as icon
  if (props.imageFormat !== ImageFormat.bmp && props.imageFormat !== ImageFormat.gif &&
    props.imageFormat !== ImageFormat.jpg && props.imageFormat !== ImageFormat.png) {
    return (<div ref={target} className={`img-box--error img-box--unsupported img-box--${props.imageFormat}`} />);
  }

  return (
    <div ref={target} className={error ? "img-box--error" : isLoading ? "img-box img-box--loading" : "img-box"}>
      {intersected ? <img src={src} alt={alt}
        onLoad={() => {
          setError(false);
          setIsLoading(false)
        }}
        onError={() => setError(true)} /> : <div className="img-box--loading" />}
    </div>
  );

});

export default ListImage
