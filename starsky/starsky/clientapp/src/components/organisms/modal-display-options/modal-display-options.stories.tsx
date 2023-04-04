import React from "react";
import ModalDisplayOptions from "./modal-display-options";

export default {
  title: "components/organisms/modal-display-options"
};

export const Default = () => {
  return (
    <ModalDisplayOptions
      isOpen={true}
      handleExit={() => {}}
    ></ModalDisplayOptions>
  );
};

Default.story = {
  name: "default"
};
