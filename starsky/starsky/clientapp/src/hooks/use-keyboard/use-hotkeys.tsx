import "core-js/features/string/match"; // event.key.match
import { useEffect } from "react";
import { Keyboard } from "../../shared/keyboard";

export interface IHotkeysKeyboardEvent {
  key: string;
  altKey?: boolean;
  ctrlKey?: boolean;
  metaKey?: boolean;
  shiftKey?: boolean;
  ctrlKeyOrMetaKey?: boolean;
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
 * @param _dependencies - deps array 
 */
function useHotKeys(
  predefined: IHotkeysKeyboardEvent = { key: "" },
  callback: (event: KeyboardEvent) => void = () => {
    /* should do nothing, you should overwrite this */
  },
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  _dependencies: any = [],
) {
  useEffect(() => {
    const handler = function (event: KeyboardEvent) {
      if (new Keyboard().isInForm(event)) return;
      if (!predefined?.key) {
        return;
      }

      const {
        key: eventKey,
        altKey: eventAltKey,
        ctrlKey: eventCtrlKey,
        metaKey: eventMetaKey,
        shiftKey: eventShiftKey,
      } = event;

      const {
        key: preDefinedKey,
        altKey: preDefinedAltKey = false,
        ctrlKey: preDefinedCtrlKey = false,
        metaKey: preDefinedMetaKey = false,
        shiftKey: preDefinedShiftKey = false,
        ctrlKeyOrMetaKey: preDefinedCtrlKeyOrMetaKey = false,
      } = predefined;

      if (
        eventKey === preDefinedKey &&
        eventAltKey === preDefinedAltKey &&
        eventShiftKey === preDefinedShiftKey &&
        preDefinedCtrlKeyOrMetaKey &&
        !preDefinedCtrlKey &&
        !preDefinedMetaKey &&
        (eventCtrlKey || eventMetaKey)
      ) {
        event.preventDefault();
        callback(event);
        return;
      }

      if (
        !preDefinedCtrlKeyOrMetaKey &&
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
     
  });
}

export default useHotKeys;
// credits: https://medium.com/@nicolaslopezj/reusing-logic-with-react-hooks-8e691f7352fa
