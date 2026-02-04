import capturePosition from "../../../hooks/use-capture-position";
import { toggleTabIndex } from "./toggle-tab-index";

export function modalFreezeOpen(
  freeze: () => void,
  exitButton: React.RefObject<HTMLButtonElement>,
  modalContainer: Element | null,
  rootContainer: Element | null
) {
  if (exitButton.current) exitButton.current.focus();
  if (modalContainer) toggleTabIndex("on", modalContainer);
  if (rootContainer) toggleTabIndex("off", rootContainer);
  freeze();
}

export function modalUnFreezeNotOpen(
  unfreeze: () => void,
  modalContainer: Element | null,
  rootContainer: Element | null,
  focusAfterExit: HTMLElement | undefined,
  initialRender: React.MutableRefObject<boolean>
) {
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

  const { freeze, unfreeze } = capturePosition();

  if (isOpen) {
    modalFreezeOpen(freeze, exitButton, modalContainer, rootContainer);
  } else {
    modalUnFreezeNotOpen(unfreeze, modalContainer, rootContainer, focusAfterExit, initialRender);
  }

  return () => {
    if (isOpen) {
      unfreeze();
    }
  };
}
