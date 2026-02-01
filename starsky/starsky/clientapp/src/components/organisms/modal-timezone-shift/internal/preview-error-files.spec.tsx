import { render, screen } from "@testing-library/react";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { IExifTimezoneCorrectionResult } from "../../../../interfaces/ITimezone";
import { PreviewErrorFiles } from "./preview-error-files";

describe("PreviewErrorFiles", () => {
  const baseItem = {
    fileIndexItem: { fileName: "test.jpg" } as unknown as IFileIndexItem,
    error: undefined,
    warning: undefined
  } as IExifTimezoneCorrectionResult;

  it("renders nothing when no errors or warnings", () => {
    render(<PreviewErrorFiles data={[baseItem]} />);
    expect(screen.queryByText(/test.jpg/)).toBeNull();
  });

  it("renders warning when present and no error", () => {
    const item = { ...baseItem, warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(screen.getByText(/⚠️ test.jpg: Minor issue/)).toBeInTheDocument();
    expect(screen.queryByText(/❌/)).toBeNull();
  });

  it("renders error when present", () => {
    const item = { ...baseItem, error: "Major issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(screen.getByText(/❌ test.jpg: Major issue/)).toBeInTheDocument();
    expect(screen.queryByText(/⚠️/)).toBeNull();
  });

  it("renders both error and warning, prioritizes error", () => {
    const item = { ...baseItem, error: "Major issue", warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(screen.getByText(/❌ test.jpg: Major issue/)).toBeInTheDocument();
    expect(screen.queryByText(/⚠️/)).toBeNull();
  });

  it("renders multiple items", () => {
    const items = [
      { ...baseItem, fileIndexItem: { fileName: "a.jpg" }, error: "E1" },
      { ...baseItem, fileIndexItem: { fileName: "b.jpg" }, warning: "W1" }
    ];
    render(<PreviewErrorFiles data={items} />);
    expect(screen.getByText(/❌ a.jpg: E1/)).toBeInTheDocument();
    expect(screen.getByText(/⚠️ b.jpg: W1/)).toBeInTheDocument();
  });
});
