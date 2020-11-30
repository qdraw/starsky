import { storiesOf } from "@storybook/react";
import React from "react";
import ModalMoveFile from './modal-move-file';

storiesOf("components/organisms/modal-move-file", module)
  .add("default", () => {
    return <ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>
  })