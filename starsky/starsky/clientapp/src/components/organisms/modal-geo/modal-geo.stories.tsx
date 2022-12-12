import { storiesOf } from "@storybook/react";
import ModalGeo from "./modal-geo";

storiesOf("components/organisms/modal-geo", module)
  .add("default", () => {
    return (
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={0}
        longitude={0}
      ></ModalGeo>
    );
  })
  .add("with location", () => {
    return (
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={51}
        longitude={3}
      ></ModalGeo>
    );
  });
