import { storiesOf } from "@storybook/react";
import React from "react";
import ModalPublish from './modal-publish';

storiesOf("components/organisms/modal-publish", module)
  .add("default", () => {
    return <ModalPublish
      isOpen={true}
      select={["/"]}
      handleExit={() => { }}>
      test
    </ModalPublish>
  })