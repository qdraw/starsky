import { fireEvent, render, screen } from "@testing-library/react";
import * as useFolderPicker from "../../../hooks/use-folder-picker";
import FolderPickerInput from "./folder-picker-input";

describe("FolderPickerInput", () => {
  beforeEach(() => jest.restoreAllMocks());

  it("renders fallback FormControl and handles blur updates", async () => {
    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue({
      isNativeApp: () => false,
      requestFolderSelection: jest.fn()
    } as unknown as {
      isNativeApp: () => boolean;
      requestFolderSelection: (cb: (p: string | null, b: string | null) => void) => void;
    });

    const onChange = jest.fn();
    const onBlur = jest.fn();

    render(
      <FolderPickerInput
        value="/initial"
        isEnabled={true}
        allowEdit={true}
        onChange={onChange}
        onBlur={onBlur}
        data-test="picker"
      />
    );

    const el = screen.getByTestId("picker");
    expect(el).toBeTruthy();
    expect(el.textContent).toContain("/initial");

    // simulate user editing the content and blurring
    el.innerText = "/newpath";
    fireEvent.blur(el);

    expect(onChange).toHaveBeenCalledWith("/newpath", null);
    expect(onBlur).toHaveBeenCalled();
  });

  it("uses native picker when available and triggers onChange", () => {
    const requestMock = jest.fn((cb: (p: string | null, b: string | null) => void) =>
      cb("/native/path", null)
    );
    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue({
      isNativeApp: () => true,
      requestFolderSelection: requestMock
    } as unknown as {
      isNativeApp: () => boolean;
      requestFolderSelection: (cb: (p: string | null, b: string | null) => void) => void;
    });

    const onChange = jest.fn();

    render(
      <FolderPickerInput
        value="/init"
        isEnabled={true}
        allowEdit={true}
        onChange={onChange}
        data-test="picker"
      />
    );

    const wrapper = screen.getByTestId("picker");
    expect(wrapper).toBeTruthy();

    const button = wrapper.querySelector("button");
    expect(button).toBeTruthy();

    fireEvent.click(button as HTMLElement);

    expect(requestMock).toHaveBeenCalled();
    expect(onChange).toHaveBeenCalledWith("/native/path", null);
  });

  it("renders disabled state when not enabled or not allowed to edit", () => {
    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue({
      isNativeApp: () => false,
      requestFolderSelection: jest.fn()
    } as unknown as {
      isNativeApp: () => boolean;
      requestFolderSelection: (cb: (p: string | null, b: string | null) => void) => void;
    });

    const onChange = jest.fn();

    // disabled by isEnabled=false
    const { rerender } = render(
      <FolderPickerInput
        value="/a"
        isEnabled={false}
        allowEdit={true}
        onChange={onChange}
        data-test="picker"
      />
    );

    let el = screen.getByTestId("picker");
    expect(el.getAttribute("contenteditable")).toBe("false");
    expect(el.className).toContain("disabled");

    // disabled by allowEdit=false
    rerender(
      <FolderPickerInput
        value="/a"
        isEnabled={true}
        allowEdit={false}
        onChange={onChange}
        data-test="picker"
      />
    );

    el = screen.getByTestId("picker");
    expect(el.getAttribute("contenteditable")).toBe("false");
    expect(el.className).toContain("disabled");
  });
});
