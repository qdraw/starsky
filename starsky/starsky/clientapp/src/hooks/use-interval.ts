import { useEffect, useRef } from 'react';

function useInterval(callback: Function, delay: number) {
  const savedCallback = useRef({} as any);

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
      let id = setInterval(tick, delay);
      console.log('----1');
      
      return () => clearInterval(id);
    }
  }, [delay]);
}

export default useInterval;