import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import Notification from "../../atoms/notification/notification";
import ColorClassSelect from "./color-class-select";
import * as ColorClassUpdateSingle from "./color-class-update-single";

describe("ColorClassSelect", () => {
  it("renders", () => {
    render(
      <ColorClassSelect
        collections={true}
        isEnabled={true}
        filePath={"/test"}
        onToggle={() => {}}
      />
    );
  });

  it("onClick value", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: [{ status: IExifStatus.Ok }] as IFileIndexItem[]
      }
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={(value) => {}}
      />
    );

    wrapper.find("button.colorclass--2").simulate("click");

    // expect
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().prefix + "/api/update",
      "f=%2Ftest1&colorclass=2&collections=true"
    );

    // Cleanup: To avoid that mocks are shared
    fetchPostSpy.mockReset();
  });

  it("onClick disabled", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      newIConnectionDefault()
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={false}
        filePath={"/test1"}
        onToggle={(value) => {}}
      />
    );

    wrapper.find("button.colorclass--2").simulate("click");

    // expect [disabled]
    expect(fetchPostSpy).toHaveBeenCalledTimes(0);

    // Cleanup: To avoid that mocks are shared
    fetchPostSpy.mockReset();
  });

  it("test hide 1 second", async () => {
    jest.useFakeTimers();
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: [{ status: IExifStatus.Ok }] as IFileIndexItem[]
      }
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={(value) => {}}
      >
        t
      </ColorClassSelect>
    );

    // need to await this click
    await act(async () => {
      await wrapper.find("button.colorclass--3").simulate("click");
    });

    wrapper.update();

    expect(wrapper.exists("button.colorclass--3.active")).toBeTruthy();

    // need to await this
    await act(async () => {
      await jest.advanceTimersByTime(1200);
    });

    wrapper.update();

    expect(wrapper.exists("button.colorclass--3.active")).toBeFalsy();

    wrapper.unmount();
    fetchPostSpy.mockReset();
    jest.useRealTimers();
  });

  it("onClick readonly file", async () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 404,
        data: [{ status: IExifStatus.ReadOnly }] as IFileIndexItem[]
      }
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={(value) => {}}
      >
        t
      </ColorClassSelect>
    );

    // need to await this click
    await act(async () => {
      await wrapper.find("button.colorclass--2").simulate("click");
    });

    wrapper.update();

    expect(wrapper.exists(Notification)).toBeTruthy();

    act(() => {
      wrapper.unmount();
    });

    fetchPostSpy.mockReset();
  });

  it("when error is it should able to close the warning box", async () => {
    const colorClassUpdateSingleSpy = jest
      .spyOn(ColorClassUpdateSingle, "ColorClassUpdateSingle")
      .mockImplementationOnce((p1, p2, p3, p4, setIsError) => {
        setIsError("true");
        return { Update: jest.fn() } as any;
      });

    const component = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={() => {}}
      />
    );

    // need to await this click
    await act(async () => {
      await component.find("button.colorclass--2").simulate("click");
    });

    expect(colorClassUpdateSingleSpy).toBeCalled();

    component.update();

    expect(component.exists(".notification")).toBeTruthy();

    component.find(".icon--close").simulate("click");

    expect(component.exists(".notification")).toBeFalsy();

    await component.unmount();
  });
});
