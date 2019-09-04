import { useEffect, useRef, useState } from 'react';
import shallowEqual from '../shared/shallow-equal';
export type IntersectionChangeHandler = (entry: IntersectionObserverEntry) => void;

// Polyfill needed for Safari 12.0 and older (12.1+ has native support)
require('intersection-observer');

// credits for: https://github.com/cats-oss/use-intersection

export type IntersectionOptions = {
  root?: React.RefObject<Element>;
  rootMargin?: string;
  threshold?: number | number[];
  once?: boolean;
  defaultIntersecting?: boolean;
};

export const useIntersection = (
  ref: React.RefObject<Element>,
  options: IntersectionOptions = {},
  callback?: IntersectionChangeHandler,
) => {
  const { defaultIntersecting, once, ...opts } = options;
  const optsRef = useRef(opts);
  const [intersecting, setIntersecting] = useState(defaultIntersecting === true);

  useEffect(() => {
    if (!shallowEqual(optsRef.current, opts)) {
      optsRef.current = opts;
    }
  });

  useEffect(() => {
    if (ref.current == null) {
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        setIntersecting(entry.isIntersecting);

        if (callback != null) {
          callback(entry);
        }

        if (once && entry.isIntersecting && ref.current != null) {
          observer.unobserve(ref.current);
        }
      },
      {
        ...optsRef.current,
        root: optsRef.current.root != null ? optsRef.current.root.current : null,
      },
    );

    observer.observe(ref.current);

    return () => {
      if (!once && ref.current != null) {
        observer.unobserve(ref.current);
      }
    };
  }, [optsRef.current]);

  return intersecting;
};

export default useIntersection;