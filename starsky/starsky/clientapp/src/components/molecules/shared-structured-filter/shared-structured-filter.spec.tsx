import { fireEvent, render, screen } from "@testing-library/react";
import SharedStructuredFilter from "./shared-structured-filter";

describe("SharedStructuredFilter", () => {
  it("shows collapsed empty state by default", () => {
    render(<SharedStructuredFilter urlObject={{}} onChange={jest.fn()} />);

    expect(screen.getByTestId("shared-filter-toggle")).toBeTruthy();
    expect(screen.queryByTestId("shared-filter-panel")).toBeNull();
  });

  it("opens panel from toggle", () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{}} onChange={onChange} />);

    fireEvent.click(screen.getByTestId("shared-filter-toggle"));

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true });
  });

  it("opens panel when url state has active filters", () => {
    render(
      <SharedStructuredFilter
        urlObject={{ imageFormat: "jpg", keywords: ["tag1"] }}
        onChange={jest.fn()}
      />
    );

    expect(screen.getByTestId("shared-filter-panel")).toBeTruthy();
    expect(screen.getByTestId("shared-filter-reset")).toBeTruthy();
  });

  it("reset clears structured filters", () => {
    const onChange = jest.fn();
    render(
      <SharedStructuredFilter
        urlObject={{ imageFormat: "jpg", camera: "Canon", dateFrom: "2026-04-01", filtersOpen: true }}
        onChange={onChange}
      />
    );

    fireEvent.click(screen.getByTestId("shared-filter-reset"));

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: false });
  });

  it("file type button updates image format", () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    fireEvent.click(screen.getByTestId("shared-filter-filetype-jpg"));

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, imageFormat: "jpg" });
  });
});