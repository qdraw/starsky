import { render, waitFor } from "@testing-library/react";
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
import * as Notification from "../../atoms/notification/notification";
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

    const wrapper = render(
      <ColorClassSelect
        collections={true}
        clearAfter={true}
        isEnabled={true}
        filePath={"/test1"}
        onToggle={(value) => {}}
      />
    );

    const colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    colorClass.click();

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

    const colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    colorClass.click();

    // expect(colorClass).toBeTruthy();

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

    let colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    await colorClass.click();

    colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;

    expect(colorClass.classList).toContain("active");

    // need to await this
    await act(async () => {
      await jest.advanceTimersByTime(1200);
    });

    colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;

    expect(colorClass.classList).not.toContain("active");

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
      />
    );

    const notificationSpy = jest
      .spyOn(Notification, "default")
      .mockImplementationOnce(() => <></>);

    const colorClass = wrapper.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    await colorClass.click();

    expect(fetchPostSpy).toBeCalled();
    expect(notificationSpy).toBeCalled();
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

    const notificationSpy = jest
      .spyOn(Notification, "default")
      .mockImplementationOnce(() => <></>);

    // need to await this click
    let colorClass = component.queryByTestId(
      "color-class-select-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    await colorClass.click();

    await waitFor(() => expect(colorClassUpdateSingleSpy).toBeCalled());

    console.log(component.container.innerHTML);

    expect(notificationSpy).toBeCalled();

    await component.unmount();
  });
});
