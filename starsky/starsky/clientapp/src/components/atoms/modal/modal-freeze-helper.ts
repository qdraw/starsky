import capturePosition from "../../../hooks/use-capture-position";

export default function modalFreezeHelper(
  initialRender: React.MutableRefObject<boolean>,
  root: string,
  id: string,
  isOpen: boolean,
  exitButton: React.RefObject<HTMLButtonElement>,
  focusAfterExit: HTMLElement | undefined
) {
  const rootContainer = document.querySelector(`#${root}`);
  const modalContainer = document.querySelector(`#${id}`);

  const toggleTabIndex = (type: "on" | "off", container: Element) => {
    const focusableElements = container.querySelectorAll(
      "button, a, input, textarea, select"
    );
    focusableElements.forEach((element: Element) => {
      if (type === "on") {
        element.removeAttribute("tabindex");
      } else {
        element.setAttribute("tabindex", "-1");
      }
    });
  };

  const { freeze, unfreeze } = capturePosition();

  if (isOpen) {
    if (exitButton.current) exitButton.current.focus();
    if (modalContainer) toggleTabIndex("on", modalContainer);
    if (rootContainer) toggleTabIndex("off", rootContainer);
    freeze();
  } else {
    if (modalContainer) toggleTabIndex("off", modalContainer);
    if (rootContainer) toggleTabIndex("on", rootContainer);
    unfreeze();
    if (focusAfterExit) focusAfterExit.focus();

    if (!initialRender.current) {
      initialRender.current = true;
      setTimeout(() => {
        if (modalContainer) toggleTabIndex("off", modalContainer);
      }, 0);
    }
  }

  return () => {
    if (isOpen) {
      unfreeze();
    }
  };
}
