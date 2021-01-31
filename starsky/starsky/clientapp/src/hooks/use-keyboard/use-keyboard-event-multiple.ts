import { useEffect, useState } from "react";

/**
 * returns
 */
export default function useKeyboardEventMultiple() {
  const [keysPressed, setKeyPressed] = useState(new Set(new Array<string>()));

  function downHandler({ key }: KeyboardEvent) {
    setKeyPressed(keysPressed.add(key));
  }

  const upHandler = ({ key }: KeyboardEvent) => {
    keysPressed.delete(key);
    setKeyPressed(keysPressed);
  };

  useEffect(() => {
    window.addEventListener("keydown", downHandler);
    window.addEventListener("keyup", upHandler);
    return () => {
      window.removeEventListener("keydown", downHandler);
      window.removeEventListener("keyup", upHandler);
    };

    // Empty array ensures that effect is only run on mount and unmount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return keysPressed;
}
// credits: https://usehooks.com/useKeyPress/
