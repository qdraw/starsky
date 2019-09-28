import React, { memo, useEffect, useRef, useState } from 'react';
import useIntersection from '../hooks/use-intersection-observer';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';

interface IListImageProps {
  src: string;
  alt?: string;
}

const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {

  const target = useRef<HTMLDivElement>(null);
  var alt = props.alt ? props.alt : 'afbeelding';

  const [src, setSrc] = useState(props.src);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  // to update the url, and trigger loading if the url is changed
  useEffect(() => {
    setError(false);
    setIsLoading(true);
  }, [props.src]);

  var intersected = useIntersection(target, {
    rootMargin: '250px',
    once: true
  })

  // to stop loading images after a url change
  var history = useLocation();
  const [historyLocation] = useState(history.location.search);
  useEffect(() => {
    // use ?f only to support details
    if (new URLPath().getFilePath(historyLocation) !== new URLPath().getFilePath(history.location.search)
      && isLoading) {
      // data:images are blocked by a strict CSP 
      setSrc('./images/empty-image.gif') // 26 bytes
    }
  }, [history.location.search]);

  if (!props.src || props.src.toLowerCase().endsWith('null.jpg?issingleitem=true')) {
    return (<div ref={target} className="img-box--error"></div>);
  }

  return (
    <div ref={target} className={error ? "img-box--error" : isLoading ? "img-box img-box--loading" : "img-box"}>
      {intersected ? <img src={src} alt={alt}
        onLoad={() => {
          setError(false)
          setIsLoading(false)
        }}
        onError={() => setError(true)} /> : <div className="img-box--loading"></div>}
    </div>
  );

});

export default ListImage