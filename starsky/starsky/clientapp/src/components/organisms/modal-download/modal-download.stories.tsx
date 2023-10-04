import ModalDownload from "./modal-download";

export default {
  title: "components/organisms/modal-download"
};

export const Default = () => {
  return (
    <ModalDownload
      isOpen={true}
      collections={false}
      select={["/"]}
      handleExit={() => {}}
    ></ModalDownload>
  );
};

Default.story = {
  name: "default"
};
