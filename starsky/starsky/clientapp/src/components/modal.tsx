import React from "react";
import ReactDOM from "react-dom";

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
  const [hasUpdated, forceUpdate] = React.useState(false);

  const exitButton = React.useRef<HTMLButtonElement>(null);
  const modal = React.useRef<HTMLDivElement | null>(null);

  React.useEffect(() => {
    modal.current = document.createElement("div");
    modal.current.id = id;

    if (!document.body.querySelector(`#${id}`)) {
      document.body.prepend(modal.current);
    }

    if (!hasUpdated) forceUpdate(true);

    return () => {
      if (modal.current) {
        document.body.removeChild(modal.current);
      }
    };
  }, []);

  const initialRender = React.useRef(false);
  React.useEffect(() => {
    const rootContainer = document.querySelector(`#${root}`);
    const modalContainer = document.querySelector(`#${id}`);

    const capturePosition = () => {
      const cachedPosition = window.pageYOffset;
      return {
        freeze: () => {
          // @ts-ignore
          document.body.style =
            `position: fixed; top: ${cachedPosition * -1}px; width: 100%;`;
        },
        unfreeze: () => {
          document.body.removeAttribute("style");
          window.scrollTo({
            top: cachedPosition
          });
        }
      };
    };

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

    const handleKeyDown = (e: any) => {
      // if (e.key === "Escape") {
      //   handleExit();
      // }
    };

    const { freeze, unfreeze } = capturePosition();

    if (isOpen) {
      if (exitButton.current) exitButton.current.focus();
      if (modalContainer) toggleTabIndex("on", modalContainer);
      if (rootContainer) toggleTabIndex("off", rootContainer);
      window.addEventListener("keydown", handleKeyDown);
      freeze();
    } else {
      if (modalContainer) toggleTabIndex("off", modalContainer);
      if (rootContainer) toggleTabIndex("on", rootContainer);
      window.removeEventListener("keydown", handleKeyDown);
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
        window.removeEventListener("keydown", handleKeyDown);
        unfreeze();
      }
    };
  }, [isOpen]);

  if (modal.current) {
    return ReactDOM.createPortal(
      <>
        <div onClick={(event) => {
          var target = event.target as HTMLElement
          if (target.className.indexOf("modal-bg--open") === -1) return;
          handleExit()
        }
        } className={`modal-bg ${isOpen ? " modal-bg--open" : ""}`}>
          <div
            className={`modal-content ${
              isOpen ? " modal-content--show" : ""
              }`}
          >
            <div className="modal-close-bar">
              <button
                className={`modal-exit-button ${isOpen ? " modal-exit-button--showing" : ""}`}
                ref={exitButton}
                onClick={() => {
                  handleExit()
                }}
              >
                Sluiten
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