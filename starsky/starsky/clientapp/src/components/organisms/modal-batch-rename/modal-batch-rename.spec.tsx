import { render } from "@testing-library/react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import * as Modal from "../../atoms/modal/modal";
import ModalBatchRename from "./modal-batch-rename";

describe("ModalBatchRename", () => {
  it("should render modal when isOpen is true", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg", "/test2.jpg"];
    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      });

    const { container } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={{} as unknown as IArchiveProps}
        undoSelection={jest.fn()}
      />
    );

    const modal = container.querySelector("[data-test='modal-batch-rename']");
    expect(modal).toBeTruthy();
    expect(modalSpy).toHaveBeenCalled();
  });

  it("should display selected file count", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg", "/test2.jpg", "/test3.jpg"];
    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      });

    const { getByText } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={{} as unknown as IArchiveProps}
        undoSelection={jest.fn()}
      />
    );

    expect(getByText(/3 photos to rename/i)).toBeInTheDocument();
    expect(modalSpy).toHaveBeenCalled();
  });

  it("should not render when isOpen is false", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    const { container } = render(
      <ModalBatchRename
        isOpen={false}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={{} as unknown as IArchiveProps}
        undoSelection={jest.fn()}
      />
    );

    const modal = container.querySelector("[data-test='modal-batch-rename']");
    expect(modal).not.toBeInTheDocument();
  });
});
