import { storiesOf } from "@storybook/react";
import React from "react";
import ModalDisplayOptions from './modal-display-options';

storiesOf("components/organisms/modal-display-options", module)
  .add("default", () => {
    return <ModalDisplayOptions
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalDisplayOptions>
  })