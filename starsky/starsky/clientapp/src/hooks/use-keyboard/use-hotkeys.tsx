import "core-js/features/string/match"; // event.key.match
import { useEffect } from "react";
import { Keyboard } from "../../shared/keyboard";

export interface IHotkeysKeyboardEvent {
  key: string;
  altKey?: boolean;
  ctrlKey?: boolean;
  metaKey?: boolean;
  shiftKey?: boolean;
}
/**
 * Use key with alt, ctrl, command or shift key
 * ```
 *  useHotKeys(
      {
        key: "q",
        altKey: true
      },
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
function useHotKeys(
  predefined: IHotkeysKeyboardEvent = { key: "" },
  callback: (event: KeyboardEvent) => void,
  dependencies: any = []
) {
  useEffect(() => {
    const handler = function (event: KeyboardEvent) {
      if (new Keyboard().isInForm(event)) return;

      if (!predefined || !predefined.key) {
        return;
      }

      const {
        key: eventKey,
        altKey: eventAltKey,
        ctrlKey: eventCtrlKey,
        metaKey: eventMetaKey,
        shiftKey: eventShiftKey
      } = event;

      const {
        key: preDefinedKey,
        altKey: preDefinedAltKey = false,
        ctrlKey: preDefinedCtrlKey = false,
        metaKey: preDefinedMetaKey = false,
        shiftKey: preDefinedShiftKey = false
      } = predefined;

      if (
        eventKey === preDefinedKey &&
        eventAltKey === preDefinedAltKey &&
        eventCtrlKey === preDefinedCtrlKey &&
        eventMetaKey === preDefinedMetaKey &&
        eventShiftKey === preDefinedShiftKey
      ) {
        event.preventDefault();
        callback(event);
      }
    };
    window.addEventListener("keydown", handler);
    return () => {
      window.removeEventListener("keydown", handler);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencies, predefined, callback]);
}

export default useHotKeys;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa
