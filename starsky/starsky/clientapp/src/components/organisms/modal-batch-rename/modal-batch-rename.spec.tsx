import { render } from "@testing-library/react";
import ModalBatchRename from "./modal-batch-rename";

describe("ModalBatchRename", () => {
  it("should render modal when isOpen is true", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg", "/test2.jpg"];

    const { container } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        selectedFilePaths={selectedFilePaths}
      />
    );

    const modal = container.querySelector("[data-test='modal-batch-rename']");
    expect(modal).toBeInTheDocument();
  });

  it("should display selected file count", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg", "/test2.jpg", "/test3.jpg"];

    const { getByText } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        selectedFilePaths={selectedFilePaths}
      />
    );

    expect(getByText(/3 photos to rename/i)).toBeInTheDocument();
  });

  it("should not render when isOpen is false", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    const { container } = render(
      <ModalBatchRename
        isOpen={false}
        handleExit={handleExit}
        selectedFilePaths={selectedFilePaths}
      />
    );

    const modal = container.querySelector("[data-test='modal-batch-rename']");
    expect(modal).not.toBeInTheDocument();
  });
});
