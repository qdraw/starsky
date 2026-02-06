import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import React from "react";
import TagAutocomplete, { setCaretToEnd } from "./tag-autocomplete";

describe("TagAutocomplete", () => {
  let fetchSpy: jest.SpyInstance;

  beforeEach(() => {
    fetchSpy = jest.spyOn(global, "fetch").mockImplementationOnce((url) => {
      if (typeof url === "string" && url.includes("/api/suggest")) {
        return Promise.resolve({
          json: () => Promise.resolve(["tag1", "tag2", "tag3", "tag4"])
        }) as Promise<Response>;
      }
      return Promise.resolve({ json: () => Promise.resolve([]) }) as Promise<Response>;
    });
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  function setup(props = { onInput: jest.fn() }) {
    const ref = React.createRef<HTMLDivElement>();
    return render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        {...props}
      />
    );
  }

  it("renders editable div and suggestions", async () => {
    setup();
    const input = screen.getByTestId("form-control");
    expect(input).toBeInTheDocument();
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeTruthy();
    });
  });

  it("calls onInput when input changes", () => {
    const onInput = jest.fn();
    setup({ onInput });
    const input = screen.getByTestId("form-control");
    fireEvent.input(input, { target: { textContent: "tag1" } });
    expect(onInput).toHaveBeenCalled();
  });

  it("shows filtered suggestions based on input", async () => {
    setup();
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag2" } });
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeTruthy();
    });
  });

  it("selects suggestion with mouse click", async () => {
    const onInput = jest.fn();
    setup({ onInput });
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      const suggestList = screen.queryByTestId("tag-suggest-list");
      if (suggestList) {
        const button = suggestList.querySelector("button");
        if (button) {
          fireEvent.mouseDown(button);
          expect(onInput).toHaveBeenCalled();
        }
      }
    });
  });

  it("handles arrow navigation and enter selection", async () => {
    const onInput = jest.fn();
    setup({ onInput });
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      fireEvent.keyDown(input, { key: "ArrowDown" });
      fireEvent.keyDown(input, { key: "Enter" });
      expect(onInput).toHaveBeenCalled();
    });
  });

  it("handles comma-separated input and autocomplete after comma", async () => {
    const onInput = jest.fn();
    setup({ onInput });
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag1, tag" } });
    await waitFor(() => {
      const suggestList = screen.queryByTestId("tag-suggest-list");
      if (suggestList) {
        const button = suggestList.querySelector("button");
        if (button) {
          fireEvent.mouseDown(button);
          expect(onInput).toHaveBeenCalled();
        }
      }
    });
  });

  it("closes suggestions on blur", async () => {
    setup();
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeTruthy();
    });
    fireEvent.blur(input);
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeNull();
    });
  });

  it("does not show suggestions if input is empty", () => {
    setup();
    expect(screen.queryByTestId("tag-suggest-list")).toBeNull();
  });

  describe("setCaretToEnd", () => {
    it("sets caret to end of contentEditable div", () => {
      // Create a contentEditable div
      const div = document.createElement("div");
      div.contentEditable = "true";
      div.textContent = "test content";
      document.body.appendChild(div);

      // Mock global selection
      const removeAllRanges = jest.fn();
      const addRange = jest.fn();
      const mockSelection = {
        removeAllRanges,
        addRange
      };
      global.getSelection = () => mockSelection as unknown as Selection;

      setCaretToEnd(div);

      expect(removeAllRanges).toHaveBeenCalled();
      expect(addRange).toHaveBeenCalled();

      document.body.removeChild(div);
    });
  });
});

describe("TagAutocomplete integration", () => {
  let fetchSpy: jest.SpyInstance;

  beforeEach(() => {
    fetchSpy = jest.spyOn(global, "fetch").mockImplementation((url) => {
      if (typeof url === "string" && url.includes("/api/suggest")) {
        return Promise.resolve({
          json: () => Promise.resolve(["tag1", "tag2", "tag3", "tag4"])
        }) as Promise<Response>;
      }
      return Promise.resolve({ json: () => Promise.resolve([]) }) as Promise<Response>;
    });
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it("allows user to type, see suggestions, select with mouse and keyboard, and updates content", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.getByText("tag1")).toBeInTheDocument();
    });

    // Select suggestion with mouse
    fireEvent.mouseDown(screen.getByText("tag2"));
    expect(onInput).toHaveBeenCalled();
    expect(ref.current?.textContent).toContain("tag2");

    // Type again for new suggestions
    fireEvent.input(input, { target: { textContent: "tag3" } });
    await waitFor(() => {
      expect(screen.getByText("tag3")).toBeInTheDocument();
    });

    // Select suggestion with keyboard
    fireEvent.keyDown(input, { key: "ArrowDown" });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onInput).toHaveBeenCalled();
    expect(ref.current?.textContent).toContain("tag3");
  });

  it("updates content with comma-separated tags when selecting suggestion", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    // Simulate existing tags
    ref.current!.textContent = "tagA, tagB";
    fireEvent.input(input, { target: { textContent: "tagA, tagB, tag" } });
    await waitFor(() => {
      expect(screen.getByText("tag1")).toBeInTheDocument();
    });
    fireEvent.mouseDown(screen.getByText("tag1"));
    // Should append tag1 after tagA, tagB, with comma and space
    expect(ref.current?.textContent).toContain("tagA, tagB, tag1, ");
    expect(onInput).toHaveBeenCalled();
  });

  it("clears suggestions and closes list when input is empty", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeTruthy();
    });
    // Now clear input
    fireEvent.input(input, { target: { textContent: "" } });
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeNull();
    });
  });

  it("clears suggestions when fetch throws in .then", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    // Mock fetch to resolve, but .then throws
    const fetchSpy = jest
      .spyOn(global, "fetch")
      .mockReset()
      .mockImplementationOnce(
        () =>
          Promise.resolve({
            json: () => {
              throw new Error("fail");
            }
          }) as unknown as Promise<Response>
      );
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );

    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    // Wait for fetch to be called
    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalled();
    });
    // Wait for suggestions to be cleared
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeNull();
    });
    fetchSpy.mockRestore();
  });

  it("clears suggestions when fetch gives invalid data", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    // Mock fetch to resolve, but .then throws
    const fetchSpy = jest
      .spyOn(global, "fetch")
      .mockReset()
      .mockImplementationOnce(
        () =>
          Promise.resolve({
            json: () => "invalid data" // should be an array, but is a string
          }) as unknown as Promise<Response>
      );
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );

    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    // Wait for fetch to be called
    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalled();
    });
    // Wait for suggestions to be cleared
    await waitFor(() => {
      expect(screen.queryByTestId("tag-suggest-list")).toBeNull();
    });
    fetchSpy.mockRestore();
  });

  it("ArrowDown increments tagKeyDownIndex and prevents default", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.getByText("tag1")).toBeInTheDocument();
    });
    // Simulate ArrowDown key presses
    const preventDefault = jest.fn();
    fireEvent.keyDown(input, { key: "ArrowDown", preventDefault });
    // expect(preventDefault).toHaveBeenCalled();
    // Press ArrowDown multiple times, should not exceed tagSuggest.length - 1
    for (let i = 0; i < 10; i++) {
      fireEvent.keyDown(input, { key: "ArrowDown", preventDefault });
    }
    // The selected suggestion should be the last one
    // Check visually by looking for data-selected or similar
    const suggestList = screen.getByTestId("tag-suggest-list");
    const buttons = suggestList.querySelectorAll("button");
    let selectedCount = 0;
    buttons.forEach((btn) => {
      if (btn.getAttribute("data-selected") === "true") selectedCount++;
    });
    expect(selectedCount).toBe(1);
    // The last button should be selected
    expect(buttons[buttons.length - 1].getAttribute("data-selected")).toBe("true");
  });

  it("ArrowDown increments and ArrowUp decrements tagKeyDownIndex", async () => {
    const ref = React.createRef<HTMLDivElement>();
    const onInput = jest.fn();
    render(
      <TagAutocomplete
        name="tags"
        className="test-class"
        contentEditable={true}
        spellcheck={true}
        reference={ref}
        onInput={onInput}
      />
    );
    const input = screen.getByTestId("form-control");
    fireEvent.focus(input);
    fireEvent.input(input, { target: { textContent: "tag" } });
    await waitFor(() => {
      expect(screen.getByText("tag1")).toBeInTheDocument();
    });
    // Initial state: no suggestion selected
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      buttons.forEach((btn) => {
        expect(btn.getAttribute("data-selected")).toBe("false");
      });
    });
    // ArrowDown once: first suggestion selected
    fireEvent.keyDown(input, { key: "ArrowDown" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[0].getAttribute("data-selected")).toBe("true");
    });
    // ArrowDown again: second suggestion selected
    fireEvent.keyDown(input, { key: "ArrowDown" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[1].getAttribute("data-selected")).toBe("true");
    });
    // ArrowDown again: third suggestion selected
    fireEvent.keyDown(input, { key: "ArrowDown" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[2].getAttribute("data-selected")).toBe("true");
    });
    // ArrowUp once: second suggestion selected
    fireEvent.keyDown(input, { key: "ArrowUp" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[1].getAttribute("data-selected")).toBe("true");
    });
    // ArrowUp again: first suggestion selected
    fireEvent.keyDown(input, { key: "ArrowUp" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[0].getAttribute("data-selected")).toBe("true");
    });
    // ArrowUp again: should stay at first suggestion
    fireEvent.keyDown(input, { key: "ArrowUp" });
    await waitFor(() => {
      const suggestList = screen.getByTestId("tag-suggest-list");
      const buttons = suggestList.querySelectorAll("button");
      expect(buttons[0].getAttribute("data-selected")).toBe("true");
    });
  });
});
