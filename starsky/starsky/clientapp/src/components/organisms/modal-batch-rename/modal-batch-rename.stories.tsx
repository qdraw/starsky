import ModalBatchRename from "./modal-batch-rename";

const meta = {
  title: "components/organisms/modal-batch-rename",
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
    state: {
      fileIndexItems: [
        {
          fileName: "DSC03746.JPG"
        }
      ]
    },
    select: [
      "DSC03746.JPG",
      "DSC03747.JPG",
      "DSC03748.JPG",
      "DSC03749.JPG",
      "DSC03750.JPG",
      "DSC03751.JPG",
      "DSC03752.JPG",
      "DSC03753.JPG",
      "DSC03754.JPG",
      "DSC03755.JPG",
      "DSC03756.JPG",
      "DSC03757.JPG",
      "DSC03758.JPG"
    ]
  }
};

export const SingleFile = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    state: { fileIndexItems: [] },
    select: ["DSC03746.JPG"]
  }
};

export const FewFiles = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    state: { fileIndexItems: [] },
    select: ["DSC03746.JPG", "DSC03747.JPG", "DSC03748.JPG"]
  }
};

export const Closed = {
  args: {
    isOpen: false,
    handleExit: () => console.log("Exit clicked"),
    state: { fileIndexItems: [] },
    select: ["DSC03746.JPG"]
  }
};
