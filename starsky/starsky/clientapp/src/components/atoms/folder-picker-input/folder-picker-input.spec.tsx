import { fireEvent, render, screen } from "@testing-library/react";
import * as useFolderPicker from "../../../hooks/use-folder-picker";
import FolderPickerInput from "./folder-picker-input";

describe("FolderPickerInput", () => {
  beforeEach(() => jest.restoreAllMocks());

  it("renders fallback FormControl and handles blur updates", async () => {
    const fallbackMock = {
      isNativeApp: () => false,
      requestFolderSelection: (arg?: number | ((p: string | null, b: string | null) => void)) => {
        if (typeof arg === "function") {
          arg(null, null);
          return;
        }
        return Promise.resolve({ path: null, bookmark: null });
      }
    } as unknown as ReturnType<typeof useFolderPicker.useFolderPicker>;

    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue(fallbackMock);

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
    const requestMock = (arg?: number | ((p: string | null, b: string | null) => void)) => {
      if (typeof arg === "function") {
        arg("/native/path", null);
        return;
      }
      return Promise.resolve({ path: "/native/path", bookmark: null });
    };

    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue({
      isNativeApp: () => true,
      requestFolderSelection: requestMock
    } as unknown as ReturnType<typeof useFolderPicker.useFolderPicker>);

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
    const disabledMock = {
      isNativeApp: () => false,
      requestFolderSelection: (arg?: number | ((p: string | null, b: string | null) => void)) => {
        if (typeof arg === "function") {
          arg(null, null);
          return;
        }
        return Promise.resolve({ path: null, bookmark: null });
      }
    } as unknown as ReturnType<typeof useFolderPicker.useFolderPicker>;

    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue(disabledMock);

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
