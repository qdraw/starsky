import { render, screen } from "@testing-library/react";
import { act } from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { Keyboard } from "../../../shared/keyboard";
import { UrlQuery } from "../../../shared/url/url-query";
import ColorClassSelectKeyboard from "./color-class-select-keyboard";
import * as ColorClassUpdateSingle from "./color-class-update-single";

describe("ColorClassSelectKeyboard", () => {
  it("renders", () => {
    render(
      <ColorClassSelectKeyboard
        collections={true}
        isEnabled={true}
        filePath={"/test"}
        onToggle={() => {}}
      />
    );
  });

  it("press keyboard and should fire http request", async () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: [{ status: IExifStatus.Ok }] as IFileIndexItem[]
    });
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const component = render(
      <ColorClassSelectKeyboard
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={() => {}}
      />
    );

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "5",
      shiftKey: true
    });

    // need to await this
    await act(async () => {
      await window.dispatchEvent(event);
    });

    // expect
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().prefix + "/api/update",
      "f=%2Ftest1&colorclass=5&collections=true"
    );

    // clean
    act(() => {
      component.unmount();
    });

    fetchPostSpy.mockReset();
  });

  it("press keyboard and should NOT fire http request", async () => {
    // should after press keyboard
    const component = render(
      <ColorClassSelectKeyboard
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={() => {}}
      />
    );
    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "5",
      shiftKey: true
    });

    const colorClassUpdateSingleSpy = jest
      .spyOn(ColorClassUpdateSingle.ColorClassUpdateSingle.prototype, "Update")
      .mockImplementationOnce(() => {});

    jest.spyOn(Keyboard.prototype, "isInForm").mockImplementationOnce(() => true);

    // need to await this
    await act(async () => {
      await window.dispatchEvent(event);
    });

    expect(colorClassUpdateSingleSpy).toHaveBeenCalledTimes(0);

    await act(async () => {
      await component.unmount();
    });

    jest.spyOn(ColorClassUpdateSingle.ColorClassUpdateSingle.prototype, "Update").mockClear();
  });

  it("when error is it should able to close the warning box", async () => {
    const colorClassUpdateSingleSpy = jest
      .spyOn(ColorClassUpdateSingle, "ColorClassUpdateSingle")
      .mockImplementationOnce((_p1, _p2, _p3, _p4, setIsError) => {
        setIsError("true");
        return { Update: jest.fn() } as unknown as ColorClassUpdateSingle.ColorClassUpdateSingle;
      });

    const component = render(
      <ColorClassSelectKeyboard
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={() => {}}
      />
    );

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "5",
      shiftKey: true
    });

    // need to await this
    await act(async () => {
      await window.dispatchEvent(event);
    });

    expect(colorClassUpdateSingleSpy).toHaveBeenCalled();

    const close = screen.queryByTestId("notification-close");
    close?.click();

    expect(component.container.innerHTML).toBe("");

    await component.unmount();
  });

  it("when done is it should able to close the info box", async () => {
    const colorClassUpdateSingleSpy = jest
      .spyOn(ColorClassUpdateSingle, "ColorClassUpdateSingle")
      .mockImplementationOnce((_p1, _p2, _p3, _p4, _p5, _p6, setCurrentColorClass) => {
        setCurrentColorClass(1);
        return { Update: jest.fn() } as unknown as ColorClassUpdateSingle.ColorClassUpdateSingle;
      });

    const component = render(
      <ColorClassSelectKeyboard
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={() => {}}
      />
    );

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "5",
      shiftKey: true
    });

    // need to await this
    await act(async () => {
      await window.dispatchEvent(event);
    });

    expect(colorClassUpdateSingleSpy).toHaveBeenCalled();

    const close = screen.queryByTestId("notification-close");
    close?.click();

    expect(component.container.innerHTML).toBe("");

    await component.unmount();
  });
});
