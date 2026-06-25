import { fireEvent, render, screen } from "@testing-library/react";
import SharedStructuredFilter from "./shared-structured-filter";

describe("SharedStructuredFilter", () => {
  beforeEach(() => {
    jest.spyOn(global, "fetch").mockResolvedValue({
      json: async () => ["Canon EOS"]
    } as Response);
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

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

  it("file type button toggles off active format", () => {
    const onChange = jest.fn();
    render(
      <SharedStructuredFilter
        urlObject={{ filtersOpen: true, imageFormat: "jpg" }}
        onChange={onChange}
      />
    );

    fireEvent.click(screen.getByTestId("shared-filter-filetype-jpg"));

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true });
  });

  it("toggle button closes panel when open without active filters", () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    fireEvent.click(screen.getByTestId("shared-filter-toggle"));

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: false });
  });

  it("clears date inputs when date values are removed", () => {
    const onChange = jest.fn();
    render(
      <SharedStructuredFilter
        urlObject={{ filtersOpen: true, dateFrom: "2026-04-01", dateTo: "2026-04-30" }}
        onChange={onChange}
      />
    );

    fireEvent.change(screen.getByTestId("shared-filter-date-from"), {
      target: { value: "" }
    });
    fireEvent.change(screen.getByTestId("shared-filter-date-to"), {
      target: { value: "" }
    });

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, dateTo: "2026-04-30" });
    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, dateFrom: "2026-04-01" });
  });

  it("updates keywords and removes empty keyword list", () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    const keywords = screen.getByTestId("shared-filter-keywords");
    keywords.textContent = "tag1, tag2";
    fireEvent.input(keywords);

    keywords.textContent = "   ";
    fireEvent.input(keywords);

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, keywords: ["tag1", "tag2"] });
    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true });
  });

  it("loads camera suggestions and selects camera", async () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    const cameraInput = screen.getByRole("textbox");
    fireEvent.change(cameraInput, { target: { value: "can" } });
    fireEvent.keyDown(cameraInput, { key: "ArrowDown" });

    const option = await screen.findByText("Canon EOS");
    fireEvent.click(option);

    expect(global.fetch).toHaveBeenCalled();
    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, camera: "Canon EOS" });
  });

  it("does not fetch camera suggestions for too-short query", async () => {
    jest.useFakeTimers();
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    const cameraInput = screen.getByRole("textbox");
    fireEvent.change(cameraInput, { target: { value: "c" } });
    jest.advanceTimersByTime(350);

    expect(global.fetch).not.toHaveBeenCalled();
    jest.useRealTimers();
  });

  it("handles non-array camera suggestion response", async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      json: async () => ({ value: "invalid" })
    } as Response);

    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={jest.fn()} />);

    const cameraInput = screen.getByRole("textbox");
    fireEvent.change(cameraInput, { target: { value: "canon" } });

    const noResults = await screen.findByTestId("searchable-dropdown-no-results");
    expect(noResults).toBeTruthy();
  });

  it("supports selecting empty camera value", async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      json: async () => [""]
    } as Response);

    const onChange = jest.fn();
    const container = render(
      <SharedStructuredFilter urlObject={{ filtersOpen: true, camera: "Canon" }} onChange={onChange} />
    );

    const cameraInput = screen.getByRole("textbox");
    fireEvent.change(cameraInput, { target: { value: "can" } });

    await screen.findByTestId("searchable-dropdown-list");
    const emptyItemButton = container.container.querySelector(
      '[data-test="searchable-dropdown-item-"] button'
    ) as HTMLButtonElement;
    fireEvent.click(emptyItemButton);

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true });
  });

  it("updates date values when selecting valid date range", () => {
    const onChange = jest.fn();
    render(<SharedStructuredFilter urlObject={{ filtersOpen: true }} onChange={onChange} />);

    fireEvent.change(screen.getByTestId("shared-filter-date-from"), {
      target: { value: "2026-04-01" }
    });
    fireEvent.change(screen.getByTestId("shared-filter-date-to"), {
      target: { value: "2026-04-30" }
    });

    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, dateFrom: "2026-04-01" });
    expect(onChange).toHaveBeenCalledWith({ filtersOpen: true, dateTo: "2026-04-30" });
  });
});