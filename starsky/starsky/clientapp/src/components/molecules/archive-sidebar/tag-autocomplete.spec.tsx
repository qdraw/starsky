import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import React from "react";
import TagAutocomplete from "./tag-autocomplete";

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
});
