import React, { memo, useEffect, useRef, useState } from 'react';
import useIntersection from '../hooks/use-intersection-observer';

interface IListImageProps {
  src: string;
  alt?: string;
}

const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {

  const target = useRef<HTMLDivElement>(null);
  var alt = props.alt ? props.alt : 'afbeelding';

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  // to update the url, and trigger loading if the url is changed
  useEffect(() => {
    setError(false);
    setIsLoading(true);
  }, [props.src]);

  const intersected = useIntersection(target, {
    rootMargin: '250px',
    once: true
  });

  if (!props.src || props.src.toLowerCase().endsWith('null.jpg?issingleitem=true')) {
    return (<div ref={target} className="img-box--error"></div>);
  }

  return (
    <div ref={target} className={error ? "img-box--error" : isLoading ? "img-box img-box--loading" : "img-box"}>
      {intersected ? <img {...props} alt={alt}
        onLoad={() => {
          setError(false)
          setIsLoading(false)
        }}
        onError={() => setError(true)} /> : <div className="img-box--loading"></div>}
    </div>
  );

});

export default ListImage