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
        isFormEnabled={true}
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
        isFormEnabled={true}
      ></ModalGeo>
    );
  })
  .add("with location (readonly)", () => {
    return (
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={51}
        longitude={3}
        isFormEnabled={false}
      ></ModalGeo>
    );
  })
  .add("no location (readonly)", () => {
    return (
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={0}
        longitude={0}
        isFormEnabled={false}
      ></ModalGeo>
    );
  });
