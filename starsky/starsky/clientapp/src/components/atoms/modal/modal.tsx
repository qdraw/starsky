import "core-js/features/dom-collections/for-each";
import { ReactNode, useEffect, useRef, useState } from "react";
import ReactDOM from "react-dom";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import modalFreezeHelper from "./modal-freeze-helper";
import modalInsertPortalDiv from "./modal-insert-portal-div";

type ModalPropTypes = {
  children: ReactNode;
  root?: string;
  id?: string;
  isOpen: boolean;
  handleExit: () => any;
  focusAfterExit?: HTMLElement;
  className?: string;
  dataTest?: string;
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
  className = "",
  dataTest = "modal-bg"
}: ModalPropTypes): any {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageCloseDialog = language.text("Sluiten", "Close");

  const [forceUpdate, setForceUpdate] = useState(false);

  const exitButton = useRef<HTMLButtonElement>(null);
  const modal = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    return modalInsertPortalDiv(modal, forceUpdate, setForceUpdate, id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const initialRender = useRef(false);
  useEffect(() => {
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
      <div
        onClick={(event) => ifModalOpenHandleExit(event, handleExit)}
        role="presentation"
        onKeyDown={(event) => {
          event.key === "Enter" && handleExit();
        }}
        data-test={dataTest}
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
      </div>,
      modal.current
    );
  }
  return null;
}

// https://codesandbox.io/s/4r86m3vqv9
