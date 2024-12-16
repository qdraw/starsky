import { useEffect, useRef } from "react";

function useInterval(callback: () => void, delay: number) {
  const savedCallback = useRef<() => void | null>(null) as React.MutableRefObject<
    () => void | null
  >;

  // Remember the latest callback.
  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  // Set up the interval.
  useEffect(() => {
    function tick() {
      if (!savedCallback.current) return;
      savedCallback.current();
    }
    if (delay !== null) {
      const id = setInterval(tick, delay);
      return () => {
        window.clearInterval(id);
      };
    }
  }, [delay]);
}

export default useInterval;
