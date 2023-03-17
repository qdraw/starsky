import "core-js/features/dom-collections/for-each";
import React from "react";
import ReactDOM from "react-dom";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import modalFreezeHelper from "./modal-freeze-helper";
import modalInserPortalDiv from "./modal-insert-portal-div";

type ModalPropTypes = {
  children: React.ReactNode;
  root?: string;
  id?: string;
  isOpen: boolean;
  handleExit: () => any;
  focusAfterExit?: HTMLElement;
  className?: string;
};

export const ModalOpenClassName = "modal-bg--open";

function ifModalOpenHandleExit(
  event: React.MouseEvent<HTMLDivElement, MouseEvent>,
  handleExit: Function
) {
  const target = event.target as HTMLElement;
  if (target.className.indexOf(ModalOpenClassName) === -1) return;
  handleExit();
}

export default function Modal({
  children,
  id = "modal-root",
  root = "root",
  isOpen,
  handleExit,
  focusAfterExit,
  className = ""
}: ModalPropTypes): any {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageCloseDialog = language.text("Sluiten", "Close");

  const [hasUpdated, forceUpdate] = React.useState(false);

  const exitButton = React.useRef<HTMLButtonElement>(null);
  const modal = React.useRef<HTMLDivElement | null>(null);

  React.useEffect(() => {
    return modalInserPortalDiv(modal, hasUpdated, forceUpdate, id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const initialRender = React.useRef(false);
  React.useEffect(() => {
    return modalFreezeHelper(
      initialRender,
      root,
      id,
      isOpen,
      exitButton,
      focusAfterExit
    );
  }, [isOpen, focusAfterExit, id, root]);

  if (modal.current) {
    return ReactDOM.createPortal(
      <>
        <div
          onClick={(event) => ifModalOpenHandleExit(event, handleExit)}
          data-test="modal-bg"
          className={`modal-bg ${
            isOpen ? ` ${ModalOpenClassName} ` + className : ""
          }`}
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
                onClick={handleExit}
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
