import React from "react";
import ModalDatetime from "./modal-edit-datetime";

export default {
  title: "components/organisms/modal-datetime"
};

export const Default = () => {
  return (
    <ModalDatetime
      isOpen={true}
      subPath="/"
      handleExit={() => {}}
    ></ModalDatetime>
  );
};

Default.story = {
  name: "default"
};
