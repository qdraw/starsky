import { storiesOf } from "@storybook/react";
import React from "react";
import ModalDatetime from './modal-edit-datetime';

storiesOf("components/organisms/modal-datetime", module)
  .add("default", () => {
    return <ModalDatetime
      isOpen={true}
      subPath="/"
      handleExit={() => { }}>
      test
    </ModalDatetime>
  })