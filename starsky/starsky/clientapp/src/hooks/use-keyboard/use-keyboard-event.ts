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
  callback: (arg0: KeyboardEvent) => void,
  dependencies: any = []
) {
  useEffect(() => {
    const handler = function (event: KeyboardEvent) {
      if (regex?.exec(event.key)) {
        callback(event);
      }
    };
    window.addEventListener("keydown", handler);
    return () => {
      window.removeEventListener("keydown", handler);
    };
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [...dependencies, regex, callback]);
}

export default useKeyboardEvent;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa
