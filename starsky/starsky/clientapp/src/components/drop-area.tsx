import React, { useEffect, useState } from "react";
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import ItemTextListView from './item-text-list-view';
import Modal from './modal';



const fileTypes = ["image/jpeg", "image/jpg", "image/png"];

type ReactNodeProps = { children: React.ReactNode }

const DropArea = ({ children }: ReactNodeProps) => {

  const [dragActive, setDrag] = useState(false);
  const [dragTarget, setDragTarget] = useState(new EventTarget());
  const [isLoading, setIsLoading] = useState(false);

  const [isOpen, setOpen] = useState(false);
  const [lastUploaded, setLastUploaded] = useState(new Date());

  // used to force react to update the array
  const [uploadFilesList, setUploadFiles] = useState(newIFileIndexItemArray());


  const onDrop = (event: DragEvent) => {
    event.preventDefault();
    setDrag(false);

    if (!event.dataTransfer) return;

    const {
      dataTransfer: { files }
    } = event;

    var filesList = Array.from(files);

    console.log("Files: ", files);

    const { length } = filesList;

    if (length === 0) {
      return false;
    }

    var formData = new FormData();

    filesList.forEach(file => {
      const { size, type, name } = file;

      if (!fileTypes.includes(type)) {
        uploadFilesList.push(CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError));
        return;
      }
      if (size / 1024 / 1024 > 250) {
        uploadFilesList.push(CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError));
        return;
      }

      formData.append("files", file);
    });

    FetchPost('/import', formData).then((data) => {
      console.log('/import >= data', data);



      Array.from(data).forEach(dataItem => {
        if (!dataItem) return;
        var status = IExifStatus.Ok;
        if ((dataItem as any).status === "IgnoredAlreadyImported") {
          status = IExifStatus.IgnoredAlreadyImported;
        }
        var uploadFileObject = CastFileIndexItem(dataItem, status);
        uploadFilesList.push(uploadFileObject);

      });

      setOpen(true);
      setLastUploaded(new Date());
    });
  };

  const CastFileIndexItem = (element: any, status: IExifStatus) => {
    var uploadFileObject = newIFileIndexItem();
    uploadFileObject.fileHash = (element as any).fileHash
    uploadFileObject.filePath = (element as any).filePath;
    uploadFileObject.fileName = new URLPath().getChild(uploadFileObject.filePath);
    uploadFileObject.lastEdited = new Date().toISOString();
    uploadFileObject.status = status;
    return uploadFileObject;
  }

  const onDragEnter = (event: DragEvent) => {
    event.preventDefault();
    if (!event.target) return;

    setDrag(true);
    setDragTarget(event.target);
  };

  const onDragLeave = (event: DragEvent) => {
    event.preventDefault();
    if (event.target === dragTarget) {
      setDrag(false);
    }
  };

  const onDragOver = (event: DragEvent) => {
    event.preventDefault();
    setDrag(true);
  };


  useEffect(() => {
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

  useEffect(() => {
    console.log('resultList/eeff');

    console.log(uploadFilesList);

  }, [isOpen]);

  return (<>
    {children}
    {/* <input type="file" onChange={onDrop}></input> */}
    <Modal
      id="detailview-drop-modal"
      isOpen={isOpen}
      handleExit={() => {
        setOpen(false)
      }}>
      <ItemTextListView lastUploaded={lastUploaded} fileIndexItems={uploadFilesList}></ItemTextListView>
    </Modal>
  </>);
};
export default DropArea;
