import { useEffect, useRef, useState } from "react";
import shallowEqual from "../shared/shallow-equal";
type IntersectionChangeHandler = (entry: IntersectionObserverEntry) => void;

// Polyfill needed for Safari 12.0 and older (12.1+ has native support) - npm package: intersection-observer

// credits for: https://github.com/cats-oss/use-intersection

export type IntersectionOptions = {
  root?: React.RefObject<Element>;
  rootMargin?: string;
  threshold?: number | number[];
  once?: boolean;
  defaultIntersecting?: boolean;
};

export const newIntersectionObserver = (
  ref: React.RefObject<Element>,
  setIntersecting: React.Dispatch<React.SetStateAction<boolean>>,
  once: boolean | undefined,
  optsRef: React.MutableRefObject<IntersectionOptions>,
  callback?: IntersectionChangeHandler
): IntersectionObserver => {
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
      root: optsRef.current.root != null ? optsRef.current.root.current : null
    }
  );
  return observer;
};

const useIntersection = (
  ref: React.RefObject<Element>,
  options: IntersectionOptions = {},
  callback?: IntersectionChangeHandler
): boolean => {
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
    const observer = newIntersectionObserver(ref, setIntersecting, once, optsRef, callback);

    observer.observe(ref.current);

    return () => {
      if (!once && ref.current != null) {
        // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
        observer.unobserve(ref.current);
      }
    };
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [optsRef.current]);

  return intersecting;
};

export default useIntersection;
