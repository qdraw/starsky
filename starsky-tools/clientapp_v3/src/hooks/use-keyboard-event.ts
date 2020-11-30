import 'core-js/features/string/match'; // event.key.match
import { useEffect } from 'react';

function useKeyboardEvent(regex: RegExp, callback: Function, dependencies: any = []) {

  useEffect(() => {
    const handler = function (event: KeyboardEvent) {
      if (regex && event.key.match(regex)) {
        callback(event)
        return;
      }
    }
    window.addEventListener('keydown', handler)
    return () => {
      window.removeEventListener('keydown', handler)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencies, regex, callback])
}

export default useKeyboardEvent;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa