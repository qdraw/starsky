import { storiesOf } from "@storybook/react";
import React from "react";
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import ModalDropAreaFilesAdded from './modal-drop-area-files-added';

storiesOf("components/organisms/modal-drop-area-files-added", module)
  .add("default", () => {
    return <ModalDropAreaFilesAdded isOpen={true} uploadFilesList={[]} handleExit={() => { }} />
  })
  .add("2 items", () => {
    return <ModalDropAreaFilesAdded isOpen={true} uploadFilesList={[{ fileName: 'test.jpg', status: IExifStatus.Ok } as IFileIndexItem,
    { fileName: 'file-error.png', status: IExifStatus.FileError } as IFileIndexItem]} handleExit={() => { }} />
  })