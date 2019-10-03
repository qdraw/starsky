import React, { useEffect, useState } from "react";
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchPost from '../shared/fetch-post';
import ItemTextListView from './item-text-list-view';



const fileTypes = ["image/jpeg", "image/jpg", "image/png"];

type ReactNodeProps = { children: React.ReactNode }

const DropArea = ({ children }: ReactNodeProps) => {

  const [state, setState] = useState(new Array<string | boolean>());
  const [dragActive, setDrag] = useState(false);
  const [dragTarget, setDragTarget] = useState(new EventTarget());
  const [isLoading, setIsLoading] = useState(false);

  const [resultList, setResult] = useState(newIFileIndexItemArray());


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
        state.push("File format must be either png or jpg")
        setState(state);

        var test = CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError);
        console.log(test);

        resultList.push(CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError));
        setResult(resultList);
        return;
      }
      if (size / 1024 / 1024 > 250) {
        state.push("File size exceeded the limit of 250MB")
        setState(state);

        var test = CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError);
        console.log(test);

        resultList.push(CastFileIndexItem({ filePath: name } as any, IExifStatus.ServerError));
        setResult(resultList);

        return;
      }
      state.push(true)
      setState(state);

      formData.append("files", file);
    });

    FetchPost('/import', formData).then((data) => {
      console.log(data);

      Array.from(data).forEach(element => {
        if (!element) return;
        (element as any).pageType = "DetailView";

        var castedItem = new CastToInterface()
          .MediaDetailView(element).data.fileIndexItem;
        console.log(castedItem);

        if (!castedItem) {
          var duplicateItem = CastFileIndexItem(element, IExifStatus.IgnoredAlreadyImported)
          resultList.push(duplicateItem)
          console.log(duplicateItem);
          return
        };
        resultList.push(castedItem)
      });

      setResult(resultList);
    });
  };

  const CastFileIndexItem = (element: any, status: IExifStatus) => {
    var duplicateItem = newIFileIndexItem();
    duplicateItem.fileHash = (element as any).fileHash
    duplicateItem.filePath = (element as any).filePath;
    duplicateItem.status = status;
    return duplicateItem;
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


  return (<>
    {children}
    <ItemTextListView colorClassUsage={[]} fileIndexItems={resultList}></ItemTextListView>
  </>
  );
};
export default DropArea;
