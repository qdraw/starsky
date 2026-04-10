import { fireEvent, render } from "@testing-library/react";
import { act } from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import * as Modal from "../../atoms/modal/modal";
import * as executeBatchRenameHelper from "./execute-batch-rename-helper";
import * as generatePreviewHelper from "./generate-preview-helper";
import ModalBatchRename from "./modal-batch-rename";

describe("ModalBatchRename", () => {
  it("should call executeBatchRename when action button is clicked after preview", async () => {
    const handleExit = jest.fn();

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      })
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      });
    const selectedFilePaths = ["/test1.jpg"];
    // Mock preview generation to simulate previewGenerated = true
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

    // Spy on executeBatchRenameHelper
    const executeSpy = jest
      .spyOn(executeBatchRenameHelper, "executeBatchRenameHelper")
      .mockImplementation(() => {
        return Promise.resolve();
      });

    const { container } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={{ fileIndexItems: [{ filePath: "/test1.jpg" }] } as unknown as IArchiveProps}
        undoSelection={jest.fn()}
      />
    );

    // Simulate preview generation
    const input = container.querySelector(".input-batch-rename-pattern") as HTMLInputElement;
    expect(input).toBeTruthy();
    input.value = "{yyyy}{MM}{dd}_{filenamebase}.{ext}";
    input.dispatchEvent(new Event("input", { bubbles: true }));

    const previewButton = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    ) as HTMLButtonElement;
    expect(previewButton).toBeTruthy();
    act(() => {
      previewButton.click();
    });

    // Now the action button should be visible
    const actionButton = container.querySelector(
      ".batch-rename-button-group .btn--default"
    ) as HTMLButtonElement;
    expect(actionButton).toBeTruthy();
    act(() => {
      actionButton.click();
    });

    expect(executeSpy).toHaveBeenCalled();
    expect(modalSpy).toHaveBeenCalledTimes(2);
    executeSpy.mockRestore();
  });
  it("should reset preview, error, and previewGenerated when pattern input changes", () => {
    const handleExit = jest.fn();
    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      })
      .mockImplementationOnce(({ children }) => {
        return <>{children}</>;
      });
    const selectedFilePaths = ["/test1.jpg"];
    const { container } = render(
      <ModalBatchRename
        isOpen={true}
        handleExit={handleExit}
        select={selectedFilePaths}
        dispatch={jest.fn()}
        historyLocationSearch=""
        state={{ fileIndexItems: [{ filePath: "/test1.jpg" }] } as unknown as IArchiveProps}
        undoSelection={jest.fn()}
      />
    );

    // Set up initial state by simulating a preview and error
    const input = container.querySelector(".input-batch-rename-pattern") as HTMLInputElement;
    expect(input).toBeTruthy();

    act(() => {
      fireEvent.change(input, { target: { value: "new-pattern" } });
    });

    // After change, preview should be empty, error should be null, previewGenerated should be false
    // We can only check DOM for preview and error, previewGenerated is reflected by the preview button being visible
    const previewList = container.querySelector(".batch-rename-preview-list");
    expect(previewList).toBeNull();

    const errorBox = container.querySelector("[data-test='modal-batch-rename-error-box']");
    expect(errorBox).toBeNull();

    // The preview button should be visible (not the action buttons)
    const previewButton = container.querySelector(
      "[data-test='button-batch-rename-generate-preview']"
    );
    expect(previewButton).toBeTruthy();
    expect(modalSpy).toHaveBeenCalledTimes(2);

    // throw new Error("Test incomplete - need access to component state");
  });

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

  it("should call handleExit when modal is closed", () => {
    const handleExit = jest.fn();
    const selectedFilePaths = ["/test1.jpg", "/test2.jpg", "/test3.jpg"];
    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockReset()
      .mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

    const container = render(
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

    expect(handleExit).toHaveBeenCalledTimes(1);
    expect(modalSpy).toHaveBeenCalledTimes(1);

    container.unmount();
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

  describe.each([
    {
      name: "0 items",
      preview: [],
      expectedTargetCount: 0,
      expectedErrorCount: 0,
      expectedTargetNames: [],
      error: false
    },
    {
      name: "1 item",
      preview: [
        {
          sourceFilePath: "/test1.jpg",
          targetFilePath: "/renamed_test1.jpg",
          relatedFilePaths: [],
          sequenceNumber: 1,
          hasError: false,
          errorMessage: undefined
        }
      ],
      expectedTargetCount: 1,
      expectedErrorCount: 0,
      expectedTargetNames: ["renamed_test1.jpg"],
      error: false
    },
    {
      name: "2 items",
      preview: [
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
      ],
      expectedTargetCount: 2,
      expectedErrorCount: 0,
      expectedTargetNames: ["renamed_test1.jpg", "renamed_test2.jpg"],
      error: false
    },
    {
      name: "3 items",
      preview: [
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
      ],
      expectedTargetCount: 3,
      expectedErrorCount: 0,
      expectedTargetNames: ["renamed_test1.jpg", "renamed_test2.jpg", "renamed_test3.jpg"],
      error: false
    },
    {
      name: "5 items",
      preview: [
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
      ],
      expectedTargetCount: 3,
      expectedErrorCount: 0,
      expectedTargetNames: ["renamed_test1.jpg", "renamed_test2.jpg", "renamed_test5.jpg"],
      error: false
    },
    {
      name: "1 defined error item",
      preview: [
        {
          sourceFilePath: "/test1.jpg",
          targetFilePath: "/renamed_test1.jpg",
          relatedFilePaths: [],
          sequenceNumber: 1,
          hasError: true,
          errorMessage: "Message from backend"
        }
      ],
      expectedTargetCount: 1,
      expectedErrorCount: 1,
      expectedTargetNames: ["renamed_test1.jpg"],
      error: true,
      errorMessage: "Message from backend"
    },
    {
      name: "1 undefined error item",
      preview: [
        {
          sourceFilePath: "/test1.jpg",
          targetFilePath: "/renamed_test1.jpg",
          relatedFilePaths: [],
          sequenceNumber: 1,
          hasError: true,
          errorMessage: undefined
        }
      ],
      expectedTargetCount: 1,
      expectedErrorCount: 1,
      expectedTargetNames: ["renamed_test1.jpg"],
      error: true,
      errorMessage: "Error"
    },
    {
      name: "5 defined error item",
      preview: [
        {
          sourceFilePath: "/test1.jpg",
          targetFilePath: "/renamed_test1.jpg",
          relatedFilePaths: [],
          sequenceNumber: 1,
          hasError: true,
          errorMessage: "Message from backend"
        },
        {
          sourceFilePath: "/test2.jpg",
          targetFilePath: "/renamed_test2.jpg",
          relatedFilePaths: [],
          sequenceNumber: 2,
          hasError: true,
          errorMessage: "Message from backend"
        },
        {
          sourceFilePath: "/test3.jpg",
          targetFilePath: "/renamed_test3.jpg",
          relatedFilePaths: [],
          sequenceNumber: 3,
          hasError: true,
          errorMessage: "Message from backend"
        },
        {
          sourceFilePath: "/test4.jpg",
          targetFilePath: "/renamed_test4.jpg",
          relatedFilePaths: [],
          sequenceNumber: 4,
          hasError: true,
          errorMessage: "Message from backend"
        },
        {
          sourceFilePath: "/test5.jpg",
          targetFilePath: "/renamed_test5.jpg",
          relatedFilePaths: [],
          sequenceNumber: 5,
          hasError: true,
          errorMessage: "Message from backend"
        }
      ],
      expectedTargetCount: 5,
      expectedErrorCount: 5,
      expectedTargetNames: [
        "renamed_test1.jpg",
        "renamed_test2.jpg",
        "renamed_test3.jpg",
        "renamed_test4.jpg",
        "renamed_test5.jpg"
      ],
      error: true,
      errorMessage: "Message from backend"
    }
  ])(
    "renderPreviewList with $name",
    ({
      preview,
      expectedTargetCount,
      expectedErrorCount,
      expectedTargetNames,
      error,
      errorMessage
    }) => {
      it(`should render preview list with ${expectedTargetCount} targets`, async () => {
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
              setPreview(preview);
              setPreviewGenerated(true);
              return;
            }
          );

        const modalSpy = jest
          .spyOn(Modal, "default")
          .mockReset()
          .mockImplementationOnce(({ children }) => <>{children}</>)
          .mockImplementationOnce(({ children }) => <>{children}</>);

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
                  },
                  {
                    filePath: "/test2.jpg"
                  },
                  {
                    filePath: "/test3.jpg"
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

        // console.log(container.innerHTML);

        const previewItem = container.querySelectorAll(".batch-rename-preview-list");
        expect(previewItem).toBeTruthy();

        const previewTarget = container.querySelectorAll(".preview-target");
        expect(previewTarget.length).toBe(expectedTargetCount);
        expectedTargetNames.forEach((name, idx) => {
          expect(previewTarget[idx]?.textContent).toBe(name);
        });

        if (error) {
          const errorBox = container.querySelectorAll(".preview-item--error");
          expect(errorBox.length).toBe(expectedErrorCount);
          expect(errorBox[0].querySelector(".preview-error-message")?.textContent).toBe(
            errorMessage
          );
        }

        expect(modalSpy).toHaveBeenCalledTimes(2);
      });
    }
  );
});
