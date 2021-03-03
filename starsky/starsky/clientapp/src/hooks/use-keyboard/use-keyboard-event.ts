import "core-js/features/string/match"; // event.key.match
import { useEffect } from "react";

/**
 * Use one key at the time. The regex is an or-statement
 * ```
 * useKeyboardEvent(
      /^([ti])$/,
      (event: KeyboardEvent) => {
        // when pressing t or i
      },
      [deps]
    );
    ```
 * @param regex - the or statement
 * @param callback - function that is called
 * @param dependencies - deps array 
 */
function useKeyboardEvent(
  regex: RegExp,
  callback: Function,
  dependencies: any = []
) {
  useEffect(() => {
    const handler = function (event: KeyboardEvent) {
      if (regex && event.key.match(regex)) {
        callback(event);
      }
    };
    window.addEventListener("keydown", handler);
    return () => {
      window.removeEventListener("keydown", handler);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencies, regex, callback]);
}

export default useKeyboardEvent;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa
