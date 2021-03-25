import React, { useEffect, useState } from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import Notification from "../../atoms/notification/notification";
import Portal from "../portal/portal";
import Preloader from "../preloader/preloader";
import { UploadFiles } from "./upload-files";

export interface IDropAreaProps {
  endpoint: string;
  folderPath?: string;
  enableInputButton?: boolean;
  enableDragAndDrop?: boolean;
  className?: string;
  callback?(result: Array<IFileIndexItem>): void;
}

/**
 * Has the user a file or is dragging elements on the page
 * @param event DragEvent which should contain files
 */
const containsFiles = (event: DragEvent) => {
  if (event.dataTransfer && event.dataTransfer.types) {
    for (const type of event.dataTransfer.types) {
      if (type === "Files") {
        return true;
      }
    }
  }
  return false;
};

/**
 * Drop Area / Upload field, callback is list of uploaded files
 * @param props Endpoints, settings to enable drag 'n drop, add extra classes
 */
const DropArea: React.FunctionComponent<IDropAreaProps> = (props) => {
  const [dragActive, setDrag] = useState(false);
  const [dragTarget, setDragTarget] = useState(
    document.createElement("span") as Element
  );
  const [isLoading, setIsLoading] = useState(false);
  const [notificationStatus, setNotificationStatus] = useState("");

  /**
   * on selecting a file
   */
  const onChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) return;
    const {
      target: { files }
    } = event;

    new UploadFiles(
      setIsLoading,
      setNotificationStatus,
      props.endpoint,
      props.folderPath,
      props.callback
    ).uploadFiles(files);
  };

  /**
   * On a mouse drop
   * @param event Drag'n drop event
   */
  const onDrop = (event: DragEvent) => {
    event.preventDefault();
    setDrag(false);

    if (!event.dataTransfer) return;

    const {
      dataTransfer: { files }
    } = event;

    new UploadFiles(
      setIsLoading,
      setNotificationStatus,
      props.endpoint,
      props.folderPath,
      props.callback
    ).uploadFiles(files);
  };

  /**
   * Show different style for drag
   * @param event DragEvent
   */
  const onDragEnter = (event: DragEvent) => {
    event.preventDefault();
    if (!event.target || !containsFiles(event)) return;
    setDrag(true);
    setDragTarget(event.target as Element);
    setDropEffect(event);
  };

  /**
   * Occurs when the dragged element leaves from the drop target.
   * The target is the window in this case
   * @param event DragEvent
   */
  const onDragLeave = (event: DragEvent) => {
    event.preventDefault();
    if (!containsFiles(event) || (event.target as Element) !== dragTarget)
      return;
    setDrag(false);
  };

  /**
   * Occurs when the dragged element is over the drop target.
   * @param event DragEvent
   */
  const onDragOver = (event: DragEvent) => {
    event.preventDefault();
    if (!containsFiles(event)) return;
    setDrag(true);
    setDropEffect(event);
  };

  /**
   * to remove the plus sign (only for layout)
   */
  const setDropEffect = (event: DragEvent): void => {
    if (!event.dataTransfer) return;
    event.dataTransfer.dropEffect = "copy";
  };

  useEffect(() => {
    if (!props.enableDragAndDrop) return;

    // Bind the event listener
    window.addEventListener("dragenter", onDragEnter);
    window.addEventListener("dragleave", onDragLeave);
    window.addEventListener("dragover", onDragOver);
    window.addEventListener("drop", onDrop);

    return () => {
      // Unbind the event listener on clean up
      window.removeEventListener("dragenter", onDragEnter);
      window.removeEventListener("dragleave", onDragLeave);
      window.removeEventListener("dragover", onDragOver);
      window.removeEventListener("drop", onDrop);
    };
  });

  useEffect(() => {
    if (dragActive) {
      document.body.classList.add("drag");
      return;
    }
    document.body.classList.remove("drag");
  }, [dragActive]);

  const dropAreaId = `droparea-file-r${Math.floor(Math.random() * 30) + 1}`;

  return (
    <>
      {isLoading ? (
        <Portal>
          <Preloader isWhite={false} isOverlay={true} />
        </Portal>
      ) : null}
      {notificationStatus ? (
        <Portal>
          <Notification>{notificationStatus}</Notification>
        </Portal>
      ) : null}
      {props.enableInputButton ? (
        <>
          <input
            id={dropAreaId}
            className="droparea-file-input"
            type="file"
            multiple={true}
            onChange={onChange}
          />
          <label className={props.className} htmlFor={dropAreaId}>
            Upload
          </label>
        </>
      ) : null}
    </>
  );
};
export default DropArea;
