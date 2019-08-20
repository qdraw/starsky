import { useEffect } from 'react';

function useKeyboardEvent(regex: RegExp, callback: Function) {
  useEffect(() => {
    const handler = function (event: KeyboardEvent) {

      // if regex match
      if (regex && event.key.match(regex)) {
        callback(event)
        return;
      }

      // // string match
      // if (event.key === key) {
      //   callback(event);
      //   return;
      // }
    }
    window.addEventListener('keydown', handler)
    return () => {
      window.removeEventListener('keydown', handler)
    }
  }, [])
}

export default useKeyboardEvent;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa