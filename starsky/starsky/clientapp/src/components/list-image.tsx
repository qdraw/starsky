import React, { memo, useEffect, useRef, useState } from 'react';
import useIntersection from '../hooks/use-intersection-observer';

interface IListImageProps {
  src: string;
  alt?: string;
}

const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {

  var alt = props.alt ? props.alt : 'afbeelding';

  const target = useRef<HTMLDivElement>(null);


  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    setIsLoading(true);
    setError(false);
  }, [props]);

  const intersected = useIntersection(target, {
    rootMargin: '250px',
    once: true
  });

  if (!props.src || props.src.toLowerCase().endsWith('null?issingleitem=true')) {
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