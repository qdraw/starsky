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

  it("should select a pattern that is recent", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];
    const patterns = [
      "{yyyy}{MM}{dd}_{filenamebase}.{ext}",
      "{filenamebase}_backup.{ext}",
      "custom_{ext}"
    ];
    // Mock localStorage
    const getItemSpy = jest
      .spyOn(window.localStorage.__proto__, "getItem")
      .mockImplementation((key) => {
        if (key === "batch-rename-patterns") {
          return JSON.stringify(patterns);
        }
        return null;
      });

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container
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

    const select = container.querySelector(
      "select.select-batch-rename-patterns"
    ) as HTMLSelectElement;

    expect(select).toBeTruthy();

    // Simulate selecting the second pattern
    select.value = patterns[2];
    select.dispatchEvent(new Event("change", { bubbles: true }));

    // The input should update to the selected pattern
    const input = container.querySelector("input.input-batch-rename-pattern") as HTMLInputElement;
    expect(input).toBeTruthy();
    if (!input) throw new Error("Pattern input not found");
    expect(input.value).toBe(patterns[2]);

    console.log(container.innerHTML);

    getItemSpy.mockRestore();
    expect(modalSpy).toHaveBeenCalledTimes(3);
  });

  it("invalid recent patterns from localStorage", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];
    // Mock localStorage
    const getItemSpy = jest
      .spyOn(window.localStorage.__proto__, "getItem")
      .mockImplementation((key) => {
        if (key === "batch-rename-patterns") {
          return "invalid json";
        }
        return null;
      });

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container
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

    const select = container.querySelector(
      "select.select-batch-rename-patterns"
    ) as HTMLSelectElement;

    expect(select).toBeFalsy();

    getItemSpy.mockRestore();
    expect(modalSpy).toHaveBeenCalledTimes(1);
  });

  it("should load and display recent patterns from localStorage", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];
    const patterns = [
      "{yyyy}{MM}{dd}_{filenamebase}.{ext}",
      "{filenamebase}_backup.{ext}",
      "custom_{ext}"
    ];
    // Mock localStorage
    const getItemSpy = jest
      .spyOn(window.localStorage.__proto__, "getItem")
      .mockImplementation((key) => {
        if (key === "batch-rename-patterns") {
          return JSON.stringify(patterns);
        }
        return null;
      });

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container
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

    const select = container.querySelector(
      "select.select-batch-rename-patterns"
    ) as HTMLSelectElement;

    expect(select).toBeTruthy();

    patterns.forEach((pattern) => {
      expect(Array.from(select.options).some((opt) => opt.value === pattern)).toBe(true);
    });

    getItemSpy.mockRestore();
    expect(modalSpy).toHaveBeenCalledTimes(2);
  });
});
