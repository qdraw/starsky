import { render, screen } from "@testing-library/react";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { IExifTimezoneCorrectionResult } from "../../../../interfaces/ITimezone";
import { PreviewErrorFiles } from "./preview-error-files";

describe("PreviewErrorFiles", () => {
  const baseItem: IExifTimezoneCorrectionResult = {
    fileIndexItem: { fileName: "test.jpg" } as IFileIndexItem,
    error: "",
    warning: "",
    success: false,
    originalDateTime: "2020-01-01T00:00:00Z",
    correctedDateTime: "2020-01-01T00:00:00Z",
    delta: "0"
  };

  it("renders nothing when no errors or warnings", () => {
    render(<PreviewErrorFiles data={[baseItem] as IExifTimezoneCorrectionResult[]} />);
    expect(screen.queryByTestId("error-filename")).toBeNull();
    expect(screen.queryByTestId("warning-filename")).toBeNull();
  });

  it("renders warning when present and no error", () => {
    const item = { ...baseItem, warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    const filenameSpan = screen.getByTestId("warning-filename");
    expect(filenameSpan).toHaveClass("filename");
    expect(filenameSpan).toHaveTextContent("test.jpg");
    // Check the warning message is present in the same <p>
    expect(filenameSpan.parentElement).toHaveTextContent("Minor issue");
    expect(screen.queryByText(/Major issue/)).toBeNull();
  });

  it("renders error when present", () => {
    const item = { ...baseItem, error: "Major issue" };
    render(<PreviewErrorFiles data={[item]} />);
    const filenameSpan = screen.getByTestId("error-filename");
    expect(filenameSpan).toHaveClass("filename");
    expect(filenameSpan).toHaveTextContent("test.jpg");
    // Check the error message is present in the same <p>
    expect(filenameSpan.parentElement).toHaveTextContent("Major issue");
    expect(screen.queryByText(/Minor issue/)).toBeNull();
  });

  it("renders both error and warning, prioritizes error", () => {
    const item = { ...baseItem, error: "Major issue", warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    const filenameSpan = screen.getByTestId("error-filename");
    expect(filenameSpan).toHaveClass("filename");
    expect(filenameSpan).toHaveTextContent("test.jpg");
    // Check the error message is present in the same <p>
    expect(filenameSpan.parentElement).toHaveTextContent("Major issue");
    expect(screen.queryByText(/Minor issue/)).toBeNull();
  });

  it("renders multiple items", () => {
    const items = [
      { ...baseItem, fileIndexItem: { fileName: "a.jpg" }, error: "E1" },
      { ...baseItem, fileIndexItem: { fileName: "b.jpg" }, warning: "W1" }
    ] as IExifTimezoneCorrectionResult[];
    render(<PreviewErrorFiles data={items} />);
    const errorSpan = screen.getByTestId("error-filename");
    expect(errorSpan).toHaveClass("filename");
    expect(errorSpan).toHaveTextContent("a.jpg");
    expect(errorSpan.parentElement).toHaveTextContent("E1");
    const warningSpan = screen.getByTestId("warning-filename");
    expect(warningSpan).toHaveClass("filename");
    expect(warningSpan).toHaveTextContent("b.jpg");
    expect(warningSpan.parentElement).toHaveTextContent("W1");
  });
});
