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
    expect(screen.queryByText(/test.jpg/)).toBeNull();
  });

  it("renders warning when present and no error", () => {
    const item = { ...baseItem, warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(
      screen.getByText((content) => content.includes("test.jpg") && content.includes("Minor issue"))
    ).toBeInTheDocument();
    expect(screen.queryByText((content) => content.includes("Major issue"))).toBeNull();
  });

  it("renders error when present", () => {
    const item = { ...baseItem, error: "Major issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(
      screen.getByText((content) => content.includes("test.jpg") && content.includes("Major issue"))
    ).toBeInTheDocument();
    expect(screen.queryByText((content) => content.includes("Minor issue"))).toBeNull();
  });

  it("renders both error and warning, prioritizes error", () => {
    const item = { ...baseItem, error: "Major issue", warning: "Minor issue" };
    render(<PreviewErrorFiles data={[item]} />);
    expect(
      screen.getByText((content) => content.includes("test.jpg") && content.includes("Major issue"))
    ).toBeInTheDocument();
    expect(screen.queryByText((content) => content.includes("Minor issue"))).toBeNull();
  });

  it("renders multiple items", () => {
    const items = [
      { ...baseItem, fileIndexItem: { fileName: "a.jpg" }, error: "E1" },
      { ...baseItem, fileIndexItem: { fileName: "b.jpg" }, warning: "W1" }
    ] as IExifTimezoneCorrectionResult[];
    render(<PreviewErrorFiles data={items} />);
    expect(
      screen.getByText((content) => content.includes("a.jpg") && content.includes("E1"))
    ).toBeInTheDocument();
    expect(
      screen.getByText((content) => content.includes("b.jpg") && content.includes("W1"))
    ).toBeInTheDocument();
  });
});
