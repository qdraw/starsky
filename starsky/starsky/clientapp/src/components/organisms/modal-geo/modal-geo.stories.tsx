import ModalGeo from "./modal-geo";

export default {
  title: "components/organisms/modal-geo"
};

export const Default = () => {
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
};

Default.story = {
  name: "default"
};

export const WithLocation = () => {
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
};

WithLocation.story = {
  name: "with location"
};

export const WithLocationReadonly = () => {
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
};

WithLocationReadonly.story = {
  name: "with location (readonly)"
};

export const NoLocationReadonly = () => {
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
};

NoLocationReadonly.story = {
  name: "no location (readonly)"
};
