import React, { memo, useRef, useState } from 'react';
import useIntersection from '../hooks/use-intersection-observer';

interface IListImageProps {
  src: string;
  alt?: string;
}

const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {
  const [error, setError] = useState(false);

  if (!props.src || props.src.toLowerCase().endsWith('null?issingleitem=true')) {
    return (<div className="img-box--error"></div>);
  }

  var alt = props.alt ? props.alt : 'afbeelding';

  const target = useRef<HTMLDivElement>(null);
  const intersected = useIntersection(target, {
    rootMargin: '250px',
    once: true
  });

  return (
    <div ref={target} className={error ? "img-box--error" : "img-box"}>
      {intersected ? <img {...props} alt={alt} onError={() => setError(true)} /> : <span />}
    </div>
  );

});

export default ListImage