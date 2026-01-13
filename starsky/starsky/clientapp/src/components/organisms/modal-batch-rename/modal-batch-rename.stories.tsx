import ModalBatchRename from "./modal-batch-rename";

const meta = {
  title: "Organisms/ModalBatchRename",
  component: ModalBatchRename,
  parameters: {
    layout: "centered"
  },
  tags: ["autodocs"]
};

export default meta;

export const Default = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    selectedFilePaths: [
      "/photos/2020/DSC03746.JPG",
      "/photos/2020/DSC03747.JPG",
      "/photos/2020/DSC03748.JPG",
      "/photos/2020/DSC03749.JPG",
      "/photos/2020/DSC03750.JPG",
      "/photos/2020/DSC03751.JPG",
      "/photos/2020/DSC03752.JPG",
      "/photos/2020/DSC03753.JPG",
      "/photos/2020/DSC03754.JPG",
      "/photos/2020/DSC03755.JPG",
      "/photos/2020/DSC03756.JPG",
      "/photos/2020/DSC03757.JPG",
      "/photos/2020/DSC03758.JPG"
    ]
  }
};

export const SingleFile = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    selectedFilePaths: ["/photos/2020/DSC03746.JPG"]
  }
};

export const FewFiles = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    selectedFilePaths: [
      "/photos/2020/DSC03746.JPG",
      "/photos/2020/DSC03747.JPG",
      "/photos/2020/DSC03748.JPG"
    ]
  }
};

export const Closed = {
  args: {
    isOpen: false,
    handleExit: () => console.log("Exit clicked"),
    selectedFilePaths: ["/photos/2020/DSC03746.JPG"]
  }
};
