import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import SearchableDropdown from "./searchable-dropdown";

describe("SearchableDropdown", () => {
  const mockFetchResults = jest.fn(async (query: string) => {
    const items = ["Apple", "Apricot", "Avocado", "Banana", "Blueberry"];
    return items.filter((item) => item.toLowerCase().includes(query.toLowerCase()));
  });

  const mockOnSelect = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should render input field with placeholder", () => {
    render(<SearchableDropdown fetchResults={mockFetchResults} placeholder="Search..." />);

    const input = screen.getByPlaceholderText("Search...");
    expect(input).toBeInTheDocument();
  });

  it("should open dropdown on input focus", async () => {
    const { getByTestId } = render(
      <SearchableDropdown
        fetchResults={mockFetchResults}
        defaultItems={[{ label: "Default", value: "default" }]}
      />
    );

    const input = getByTestId("searchable-dropdown-input");
    fireEvent.focus(input);

    await waitFor(() => {
      expect(screen.getByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });
  });

  it("should fetch and display results on input change", async () => {
    const { getByTestId } = render(<SearchableDropdown fetchResults={mockFetchResults} />);

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "app" } });

    await waitFor(() => {
      expect(mockFetchResults).toHaveBeenCalledWith("app");
      expect(screen.getByTestId("searchable-dropdown-item-Apple")).toBeInTheDocument();
      // "Apricot" does not include "app" as a substring, so it should not be present
      expect(screen.queryByTestId("searchable-dropdown-item-Apricot")).not.toBeInTheDocument();
    });
  });

  it("should call onSelect when item is clicked", async () => {
    const { getByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} onSelect={mockOnSelect} />
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "app" } });

    await waitFor(() => {
      const appleItem = screen.getByTestId("searchable-dropdown-item-Apple");
      const button = appleItem.querySelector("button");
      expect(button).toBeTruthy();
      if (button) fireEvent.click(button);
    });
    // Wait for onSelect to be called
    await waitFor(() => {
      expect(mockOnSelect).toHaveBeenCalledTimes(1);
      expect(mockOnSelect).toHaveBeenCalledWith("Apple", "");
    });
  });

  it("should navigate with arrow keys", async () => {
    const { getByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} onSelect={mockOnSelect} />
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "a" } });

    await waitFor(() => {
      expect(screen.getByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });

    // Press down arrow
    fireEvent.keyDown(input, { key: "ArrowDown" });

    await waitFor(() => {
      const firstItem = screen.getByTestId("searchable-dropdown-item-Apple");
      expect(firstItem).toHaveClass("searchable-dropdown__item--selected");
    });
  });

  it("should select item on Enter key", async () => {
    const { getByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} onSelect={mockOnSelect} />
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "app" } });

    await waitFor(() => {
      expect(screen.getByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });

    fireEvent.keyDown(input, { key: "ArrowDown" });
    fireEvent.keyDown(input, { key: "Enter" });

    await waitFor(() => {
      expect(mockOnSelect).toHaveBeenCalledWith("Apple", "");
    });
  });

  it("should close dropdown on Escape key", async () => {
    const { getByTestId, queryByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} />
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "a" } });

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });

    fireEvent.keyDown(input, { key: "Escape" });

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-list")).not.toBeInTheDocument();
    });
  });

  it("should display default items when no query", async () => {
    const defaultItems = [
      { label: "Item 1", value: "item1" },
      { label: "Item 2", value: "item2" }
    ];

    const { getByTestId, queryByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} defaultItems={defaultItems} />
    );

    const input = getByTestId("searchable-dropdown-input");
    fireEvent.focus(input);

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-item-item1")).toBeInTheDocument();
      expect(queryByTestId("searchable-dropdown-item-item2")).toBeInTheDocument();
    });
  });

  it("should show no results message when no items found", async () => {
    const { getByTestId, queryByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} />
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "xyz" } });

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-no-results")).toBeInTheDocument();
    });
  });

  it("should close dropdown when clicking outside", async () => {
    const { getByTestId, queryByTestId } = render(
      <div>
        <SearchableDropdown fetchResults={mockFetchResults} />
        <button>Outside Button</button>
      </div>
    );

    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "a" } });

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });

    const outsideButton = screen.getByText("Outside Button");
    fireEvent.mouseDown(outsideButton);

    await waitFor(() => {
      expect(queryByTestId("searchable-dropdown-list")).not.toBeInTheDocument();
    });
  });

  it("should handle fetchResults error gracefully", async () => {
    const errorFetchResults = jest.fn(async () => {
      throw new Error("Network error");
    });
    render(<SearchableDropdown fetchResults={errorFetchResults} />);
    const input = screen.getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "fail" } });
    await waitFor(() => {
      // Should show no results or error message, depending on implementation
      // Here we check for the no-results message as a fallback
      expect(screen.queryByTestId("searchable-dropdown-no-results")).toBeInTheDocument();
    });
  });

  it("should navigate with ArrowDown, ArrowUp, and select with Enter", async () => {
    const { getByTestId } = render(
      <SearchableDropdown fetchResults={mockFetchResults} onSelect={mockOnSelect} />
    );
    const input = getByTestId("searchable-dropdown-input") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "a" } });
    await waitFor(() => {
      expect(screen.getByTestId("searchable-dropdown-list")).toBeInTheDocument();
    });
    // ArrowDown to first item
    fireEvent.keyDown(input, { key: "ArrowDown" });
    await waitFor(() => {
      const firstItem = screen.getByTestId("searchable-dropdown-item-Apple");
      expect(firstItem).toHaveClass("searchable-dropdown__item--selected");
    });
    // ArrowDown to second item
    fireEvent.keyDown(input, { key: "ArrowDown" });
    await waitFor(() => {
      const secondItem = screen.getByTestId("searchable-dropdown-item-Apricot");
      expect(secondItem).toHaveClass("searchable-dropdown__item--selected");
    });
    // ArrowUp back to first item
    fireEvent.keyDown(input, { key: "ArrowUp" });
    await waitFor(() => {
      const firstItem = screen.getByTestId("searchable-dropdown-item-Apple");
      expect(firstItem).toHaveClass("searchable-dropdown__item--selected");
    });
    // Enter to select
    fireEvent.keyDown(input, { key: "Enter" });
    await waitFor(() => {
      expect(mockOnSelect).toHaveBeenCalledWith("Apple", "");
    });
  });
});
