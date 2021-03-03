import React, { useEffect, useState } from "react";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import {
  IFileIndexItem,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import FetchPost from "../../../shared/fetch-post";
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

const CastFileIndexItem = (element: any): IFileIndexItem => {
  var uploadFileObject = newIFileIndexItem();
  uploadFileObject.fileHash = element.fileHash;
  uploadFileObject.filePath = element.filePath;
  uploadFileObject.isDirectory = false;
  uploadFileObject.fileName = element.fileName;
  uploadFileObject.lastEdited = new Date().toISOString();
  uploadFileObject.status = element.status;
  return uploadFileObject;
};

export function PostSingleFormData(
  endpoint: string,
  folderPath: string | undefined,
  inputFilesList: File[],
  index: number,
  outputUploadFilesList: IFileIndexItem[],
  callBackWhenReady: (result: IFileIndexItem[]) => void,
  setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
) {
  var formData = new FormData();

  if (inputFilesList.length === index) {
    setNotificationStatus("");
    callBackWhenReady(outputUploadFilesList);
    return;
  }

  setNotificationStatus(
    `Uploading ${index + 1}/${inputFilesList.length} ${
      inputFilesList[index].name
    }`
  );

  if (inputFilesList[index].size / 1024 / 1024 > 250) {
    outputUploadFilesList.push({
      filePath: inputFilesList[index].name,
      fileName: inputFilesList[index].name,
      status: IExifStatus.ServerError
    } as IFileIndexItem);
    next(
      endpoint,
      folderPath,
      inputFilesList,
      index,
      outputUploadFilesList,
      callBackWhenReady,
      setNotificationStatus
    );
    return;
  }

  formData.append("file", inputFilesList[index]);

  FetchPost(endpoint, formData, "post", { to: folderPath }).then((response) => {
    if (!response.data) {
      outputUploadFilesList.push({
        filePath: inputFilesList[index].name,
        fileName: inputFilesList[index].name,
        status: IExifStatus.ServerError
      } as IFileIndexItem);

      next(
        endpoint,
        folderPath,
        inputFilesList,
        index,
        outputUploadFilesList,
        callBackWhenReady,
        setNotificationStatus
      );
      return;
    }

    Array.from(response.data).forEach((dataItem: any) => {
      if (!dataItem) {
        outputUploadFilesList.push({
          filePath: inputFilesList[index].name,
          fileName: inputFilesList[index].name,
          status: IExifStatus.ServerError
        } as IFileIndexItem);
      } else if (
        dataItem.fileIndexItem &&
        (dataItem.status as IExifStatus) !== IExifStatus.Ok
      ) {
        outputUploadFilesList.push(CastFileIndexItem(dataItem.fileIndexItem));
      } else if (
        !dataItem.fileIndexItem &&
        (dataItem.status as IExifStatus) !== IExifStatus.Ok
      ) {
        // when `/import` already existing item
        outputUploadFilesList.push({
          filePath: dataItem.filePath,
          fileName: inputFilesList[index].name,
          isDirectory: false,
          fileHash: dataItem.fileHash,
          status: dataItem.status
        } as IFileIndexItem);
      } else {
        dataItem.fileIndexItem.lastEdited = new Date().toISOString();
        outputUploadFilesList.push(dataItem.fileIndexItem);
      }
    });

    next(
      endpoint,
      folderPath,
      inputFilesList,
      index,
      outputUploadFilesList,
      callBackWhenReady,
      setNotificationStatus
    );
  });
}

function next(
  endpoint: string,
  folderPath: string | undefined,
  inputFilesList: File[],
  index: number,
  outputUploadFilesList: IFileIndexItem[],
  callBackWhenReady: (result: IFileIndexItem[]) => void,
  setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
): void {
  index++;
  PostSingleFormData(
    endpoint,
    folderPath,
    inputFilesList,
    index,
    outputUploadFilesList,
    callBackWhenReady,
    setNotificationStatus
  );
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
          <Preloader isDetailMenu={false} isOverlay={true} />
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
