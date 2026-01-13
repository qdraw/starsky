import { render } from "@testing-library/react";
import { act } from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import * as Modal from "../../atoms/modal/modal";
import * as generatePreviewHelper from "./generate-preview-helper";
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

  it("renderPreviewList with 0 items", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(0);

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 1 item", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: false,
              errorMessage: undefined
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(1);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 2 items", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test2.jpg",
              targetFilePath: "/renamed_test2.jpg",
              relatedFilePaths: [],
              sequenceNumber: 2,
              hasError: false,
              errorMessage: undefined
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(2);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");
    expect(previewTarget[1].textContent).toBe("renamed_test2.jpg");

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 3 items", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test2.jpg",
              targetFilePath: "/renamed_test2.jpg",
              relatedFilePaths: [],
              sequenceNumber: 2,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test3.jpg",
              targetFilePath: "/renamed_test3.jpg",
              relatedFilePaths: [],
              sequenceNumber: 3,
              hasError: false,
              errorMessage: undefined
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(3);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");
    expect(previewTarget[1].textContent).toBe("renamed_test2.jpg");
    expect(previewTarget[2].textContent).toBe("renamed_test3.jpg");

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 5 items", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test2.jpg",
              targetFilePath: "/renamed_test2.jpg",
              relatedFilePaths: [],
              sequenceNumber: 2,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test3.jpg",
              targetFilePath: "/renamed_test3.jpg",
              relatedFilePaths: [],
              sequenceNumber: 3,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test4.jpg",
              targetFilePath: "/renamed_test4.jpg",
              relatedFilePaths: [],
              sequenceNumber: 4,
              hasError: false,
              errorMessage: undefined
            },
            {
              sourceFilePath: "/test5.jpg",
              targetFilePath: "/renamed_test5.jpg",
              relatedFilePaths: [],
              sequenceNumber: 5,
              hasError: false,
              errorMessage: undefined
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(3);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");
    expect(previewTarget[1].textContent).toBe("renamed_test2.jpg");
    expect(previewTarget[2].textContent).toBe("renamed_test5.jpg");

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 1 defined error item", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: true,
              errorMessage: "Message from backend"
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(1);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");

    // modal-batch-rename-error-box
    const errorBox = container.querySelectorAll(".preview-item--error");
    expect(errorBox.length).toBe(1);

    expect(errorBox[0].querySelector(".preview-error-message")?.textContent).toBe(
      "Message from backend"
    );

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });

  it("renderPreviewList with 1 undefined error item", async () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg"];

    jest
      .spyOn(generatePreviewHelper, "generatePreviewHelper")
      .mockImplementationOnce(
        async (
          _: string,
          setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
          setError: React.Dispatch<React.SetStateAction<string | null>>,
          setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
          setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>
        ) => {
          setIsPreviewLoading(false);
          setError(null);
          setPreview([
            {
              sourceFilePath: "/test1.jpg",
              targetFilePath: "/renamed_test1.jpg",
              relatedFilePaths: [],
              sequenceNumber: 1,
              hasError: true,
              errorMessage: undefined
            }
          ]);
          setPreviewGenerated(true);
          return;
        }
      );

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => <>{children}</>)
      .mockImplementationOnce(({ children }) => <>{children}</>);

    // Query the select element by class within the rendered container

    const modalBatchRename = (
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={
          {
            fileIndexItems: [
              {
                filePath: "/test1.jpg"
              }
            ]
          } as unknown as IArchiveProps
        }
        undoSelection={jest.fn()}
      />
    );
    const { container } = render(modalBatchRename);

    const input = container.querySelector(
      "[data-test='input-batch-rename-pattern']"
    ) as HTMLInputElement;

    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";

    const button = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.disabled).toBe(false);

    act(() => {
      button.click();
    });

    console.log(container.innerHTML);

    const previewItem = container.querySelectorAll(".batch-rename-preview-list");
    expect(previewItem).toBeTruthy();
    //
    const previewTarget = container.querySelectorAll(".preview-target");
    expect(previewTarget.length).toBe(1);
    expect(previewTarget[0].textContent).toBe("renamed_test1.jpg");

    // modal-batch-rename-error-box
    const errorBox = container.querySelectorAll(".preview-item--error");
    expect(errorBox.length).toBe(1);

    expect(errorBox[0].querySelector(".preview-error-message")?.textContent).toBe("Error");

    expect(modalSpy).toHaveBeenCalledTimes(2);
  });
});
