import React, { useEffect, useState } from "react";
import useGlobalSettings from '../hooks/use-global-settings';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';
import ItemTextListView from './item-text-list-view';
import Modal from './modal';
import Preloader from './preloader';

export interface IDropAreaProps {
  endpoint: string;
  folderPath?: string;
  enableInputButton?: boolean;
  enableDragAndDrop?: boolean;
  className?: string;
  callback?: Function;
}

const DropArea: React.FunctionComponent<IDropAreaProps> = (props) => {

  const settings = useGlobalSettings();
  const MessageFilesAdded = new Language(settings.language).text("Deze bestanden zijn toegevoegd", "These files have been added");

  const [dragActive, setDrag] = useState(false);
  const [dragTarget, setDragTarget] = useState(document.createElement("span") as Element);
  const [isLoading, setIsLoading] = useState(false);

  const [isOpen, setOpen] = useState(false);

  // used to force react to update the array
  const [uploadFilesList] = useState(newIFileIndexItemArray());

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

    uploadFiles(files);
  };

  /**
   * on selecting a file
   */
  const onChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) return;
    const {
      target: { files }
    } = event;

    uploadFiles(files);
  }

  /**
   * Pushing content to the server
   * @param files FileList
   */
  const uploadFiles = (files: FileList) => {
    setIsLoading(true);

    var filesList = Array.from(files);

    console.log("Files: ", files);

    const { length } = filesList;

    if (length === 0) {
      return false;
    }

    var formData = new FormData();

    filesList.forEach(file => {
      const { size, name } = file;

      if (size / 1024 / 1024 > 250) {
        uploadFilesList.push(CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError));
        return;
      }

      formData.append("files", file);
    });

    FetchPost(props.endpoint, formData, 'post', { 'to': props.folderPath }).then((response) => {
      if (!response.data) {
        setOpen(true);
        setIsLoading(false);
        return;
      }
      console.log('/import >= data', response.data);

      Array.from(response.data).forEach((dataItem: any) => {
        if (!dataItem) return;
        var status = IExifStatus.Ok;
        if (dataItem.status === "IgnoredAlreadyImported") {
          status = IExifStatus.IgnoredAlreadyImported;
        }
        else if (dataItem.status === "FileError") {
          status = IExifStatus.FileError;
        }
        var uploadFileObject = CastFileIndexItem(dataItem, status);
        uploadFilesList.push(uploadFileObject);

      });

      setOpen(true);
      setIsLoading(false);

      if (props.callback) {
        props.callback();
      }
    });
  };

  const CastFileIndexItem = (element: any, status: IExifStatus) => {
    var uploadFileObject = newIFileIndexItem();
    uploadFileObject.fileHash = element.fileHash;
    uploadFileObject.filePath = element.filePath;
    uploadFileObject.isDirectory = false;
    uploadFileObject.fileName = new URLPath().getChild(uploadFileObject.filePath);
    uploadFileObject.lastEdited = new Date().toISOString();
    uploadFileObject.status = status;
    return uploadFileObject;
  };

  /**
   * Show different style for drag
   * @param event DragEvent
   */
  const onDragEnter = (event: DragEvent) => {
    event.preventDefault();
    if (!event.target) return;

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
    if (event.target as Element === dragTarget) {
      setDrag(false);
    }
  };

  /**
  * Occurs when the dragged element is over the drop target.
  * @param event DragEvent
  */
  const onDragOver = (event: DragEvent) => {
    event.preventDefault();
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
    window.addEventListener('dragenter', onDragEnter);
    window.addEventListener('dragleave', onDragLeave);
    window.addEventListener('dragover', onDragOver);
    window.addEventListener('drop', onDrop);

    return () => {
      // Unbind the event listener on clean up
      window.removeEventListener('dragenter', onDragEnter);
      window.removeEventListener('dragleave', onDragLeave);
      window.removeEventListener('dragover', onDragOver);
      window.removeEventListener('drop', onDrop);
    };
  });

  useEffect(() => {
    if (dragActive) {
      document.body.classList.add('drag');
      return;
    }
    document.body.classList.remove('drag');
  }, [dragActive]);

  const dropareaId = `droparea-file-r${Math.floor(Math.random() * 30) + 1}`

  return (<>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : ""}

    {props.enableInputButton ? <>
      <input id={dropareaId} className="droparea-file-input" type="file" onChange={onChange} />
      <label className={props.className} htmlFor={dropareaId} >Upload</label>
    </> : null}

    <Modal
      id="detailview-drop-modal"
      isOpen={isOpen}
      handleExit={() => {
        setOpen(false)
      }}>
      <div className="modal content--subheader">{MessageFilesAdded}</div>
      <div className="modal modal-move content content--text">
        <ItemTextListView fileIndexItems={uploadFilesList} callback={() => { }} />
      </div>
    </Modal>
  </>);
};
export default DropArea;
