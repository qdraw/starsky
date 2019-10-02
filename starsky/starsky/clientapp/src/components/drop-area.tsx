import React, { useEffect, useState } from "react";
import FetchPost from '../shared/fetch-post';



const fileTypes = ["image/jpeg", "image/jpg", "image/png"];

type ReactNodeProps = { children: React.ReactNode }

const DropArea = ({ children }: ReactNodeProps) => {

  const [state, setState] = useState(new Array<string | boolean>());
  const [dragActive, setDrag] = useState(false);

  const onDrop = (event: DragEvent) => {
    event.preventDefault();
    event.stopPropagation()

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
      const { size, type } = file;

      if (!fileTypes.includes(type)) {
        state.push("File format must be either png or jpg")
        setState(state);
        return;
      }
      if (size / 1024 / 1024 > 250) {
        state.push("File size exceeded the limit of 250MB")
        setState(state);
        return;
      }
      state.push(true)
      setState(state);

      formData.append("files", file);
    });

    FetchPost('/import', formData).then((result) => {
      console.log(result);

    });
  };

  const handleDragIn = (event: DragEvent) => {
    event.preventDefault();
    event.stopPropagation();
    if (!event.dataTransfer) return;
    if (event.dataTransfer.items && event.dataTransfer.items.length > 0) {
      setDrag(true);
    }
  };

  const handleDragOut = (event: DragEvent) => {
    event.preventDefault();
    event.stopPropagation()
    setDrag(false);
  };

  // <div>
  //   <div
  //     className={drag ? "drop drop--drag" : "drop"}
  //     onDrop={e => onDrop(e)}
  //     onDragStart={e => onDragStart(e)}
  //     onDragOver={e => onDragOver(e)}
  //   >
  //   </div>
  // </div>


  useEffect(() => {
    // Bind the event listener
    document.addEventListener("drop", onDrop);
    document.addEventListener('dragenter', handleDragIn)
    document.addEventListener('dragleave', handleDragOut)
    return () => {
      // Unbind the event listener on clean up
      document.removeEventListener("drop", onDrop);
      document.removeEventListener('dragenter', handleDragIn)
      document.removeEventListener('dragleave', handleDragOut)
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
  </>
  );
};
export default DropArea;
