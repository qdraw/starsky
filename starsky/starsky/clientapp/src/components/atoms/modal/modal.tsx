import "core-js/features/dom-collections/for-each";
import React from "react";
import ReactDOM from "react-dom";
import capturePosition from "../../../hooks/use-capture-position";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";

type ModalPropTypes = {
  children: React.ReactNode;
  root?: string;
  id?: string;
  isOpen: boolean;
  handleExit: () => any;
  focusAfterExit?: HTMLElement;
};

export default function Modal({
  children,
  id = "modal-root",
  root = "root",
  isOpen,
  handleExit,
  focusAfterExit
}: ModalPropTypes): any {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageCloseDialog = language.text("Sluiten", "Close");

  const [hasUpdated, forceUpdate] = React.useState(false);

  const exitButton = React.useRef<HTMLButtonElement>(null);
  const modal = React.useRef<HTMLDivElement | null>(null);

  React.useEffect(() => {
    modal.current = document.createElement("div");
    modal.current.id = id;

    if (!document.body.querySelector(`#${id}`)) {
      document.body.insertBefore(modal.current, document.body.firstChild);
    }

    if (!hasUpdated) forceUpdate(true);

    return () => {
      if (modal.current) {
        document.body.removeChild(modal.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const initialRender = React.useRef(false);
  React.useEffect(() => {
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
  }, [isOpen, focusAfterExit, id, root]);

  if (modal.current) {
    return ReactDOM.createPortal(
      <>
        <div
          onClick={(event) => {
            const target = event.target as HTMLElement;
            if (target.className.indexOf("modal-bg--open") === -1) return;
            handleExit();
          }}
          data-test="modal-bg"
          className={`modal-bg ${isOpen ? " modal-bg--open" : ""}`}
        >
          <div
            className={`modal-content ${isOpen ? " modal-content--show" : ""}`}
          >
            <div className="modal-close-bar">
              <button
                className={`modal-exit-button ${
                  isOpen ? " modal-exit-button--showing" : ""
                }`}
                ref={exitButton}
                data-test="modal-exit-button"
                onClick={() => {
                  handleExit();
                }}
              >
                {MessageCloseDialog}
              </button>
            </div>
            {children}
          </div>
        </div>
      </>,
      modal.current
    );
  }
  return null;
}

// https://codesandbox.io/s/4r86m3vqv9
