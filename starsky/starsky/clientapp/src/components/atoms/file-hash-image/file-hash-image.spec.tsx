import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import * as DetectAutomaticRotation from "../../../shared/detect-automatic-rotation";
import * as FetchGet from "../../../shared/fetch-get";
import { UrlQuery } from "../../../shared/url-query";
import FileHashImage from "./file-hash-image";
import * as PanAndZoomImage from "./pan-and-zoom-image";

describe("FileHashImage", () => {
  it("renders", () => {
    render(<FileHashImage isError={false} fileHash={""} />);
  });

  it("Rotation API is called return 202", async () => {
    console.log("-- Rotation API is called return 202 --");

    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 202,
        data: {
          subPath: "/test/image.jpg",
          pageType: PageType.DetailView,
          fileIndexItem: {
            orientation: Orientation.Rotate270Cw,
            fileHash: "needed",
            status: IExifStatus.Ok,
            filePath: "/test/image.jpg",
            fileName: "image.jpg"
          }
        } as IDetailView
      } as IConnectionDefault);

    let detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockImplementationOnce(() => {
        return Promise.resolve(false);
      });

    var spyGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var component = render(<>test</>);
    await act(async () => {
      component = await render(
        <FileHashImage
          isError={false}
          fileHash="hash"
          orientation={Orientation.Horizontal}
        />
      );
    });

    await expect(detectRotationSpy).toBeCalled();

    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi("hash"));

    component.unmount();
  });

  it("Rotation API is called return 200", async () => {
    console.log("-- Rotation API is called return 200 --");

    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 200,
        data: {
          subPath: "/test/image.jpg",
          pageType: PageType.DetailView,
          fileIndexItem: {
            orientation: Orientation.Rotate270Cw,
            fileHash: "needed",
            status: IExifStatus.Ok,
            filePath: "/test/image.jpg",
            fileName: "image.jpg"
          }
        } as IDetailView
      } as IConnectionDefault);

    let detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockImplementationOnce(() => {
        return Promise.resolve(false);
      });

    var spyGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var component = render(<></>);
    await act(async () => {
      component = await render(
        <FileHashImage
          isError={false}
          fileHash="hash"
          orientation={Orientation.Horizontal}
        />
      );
    });

    await expect(detectRotationSpy).toBeCalled();

    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi("hash"));

    component.unmount();
  });

  it("should ignore when DetectAutomaticRotation is true", async () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 200
      } as IConnectionDefault);

    let detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

    var spyGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var component = render(<></>);
    await act(async () => {
      component = await render(
        <FileHashImage
          isError={false}
          fileHash="hash"
          orientation={Orientation.Horizontal}
        />
      );
    });

    await expect(detectRotationSpy).toBeCalled();

    expect(spyGet).toBeCalledTimes(0);

    spyGet.mockReset();
    component.unmount();
  });

  it("onWheelCallback should replace source image when event is returned", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 500
      } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const panZoomObject = (props: any) => {
      return (
        <>
          <img src={props.src} alt="test" />
          <button onClick={props.onWheelCallback}></button>
        </>
      );
    };
    jest
      .spyOn(PanAndZoomImage, "default")
      .mockImplementationOnce(panZoomObject)
      .mockImplementationOnce(panZoomObject);

    const component = render(
      <FileHashImage
        isError={false}
        fileHash="hash"
        orientation={Orientation.Horizontal}
      />
    );

    const button = component.getAllByRole("button")[0] as HTMLButtonElement;
    const image = component.getAllByRole("img")[0] as HTMLImageElement;

    act(() => {
      button.click();
    });

    expect(image.src).toBe(
      "http://localhost" +
        new UrlQuery().UrlThumbnailZoom("hash", "fallbackPath", 1)
    );
  });

  it("onWheelCallback should return callback", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 500
      } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const panZoomObject = (props: any) => {
      return (
        <>
          <img src={props.src} alt="test" />
          <button onClick={props.onWheelCallback}></button>
        </>
      );
    };
    jest
      .spyOn(PanAndZoomImage, "default")
      .mockImplementationOnce(panZoomObject)
      .mockImplementationOnce(panZoomObject);

    const onWheelCallbackSpy = jest.fn();
    const component = render(
      <FileHashImage
        isError={false}
        fileHash="hash"
        orientation={Orientation.Horizontal}
        onWheelCallback={onWheelCallbackSpy}
      />
    );

    const button = component.getAllByRole("button")[0];

    act(() => {
      button.click();
    });

    expect(onWheelCallbackSpy).toBeCalled();
  });

  it("with onResetCallback it should set UrlThumbnailImage", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 500
      } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const panZoomObject = (props: any) => {
      return (
        <>
          <img src={props.src} alt="test" />
          <button onClick={props.onResetCallback}></button>
        </>
      );
    };
    jest
      .spyOn(PanAndZoomImage, "default")
      .mockImplementationOnce(panZoomObject)
      .mockImplementationOnce(panZoomObject);

    const component = render(
      <FileHashImage
        isError={false}
        fileHash="hash"
        orientation={Orientation.Horizontal}
      />
    );

    // there is one problem with this test, is assumes the default value
    component.queryByRole("button")?.click();

    const img = component.queryByRole("img") as HTMLImageElement;
    expect(img.src).toBe(
      "http://localhost" +
        new UrlQuery().UrlThumbnailImageLargeOrExtraLarge("hash", true)
    );
  });

  it("with onResetCallback it should set UrlThumbnailImage and pass callback", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 500
      } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const panZoomObject = (props: any) => {
      return (
        <>
          <img src={props.src} alt="test" />
          <button onClick={props.onResetCallback}></button>
        </>
      );
    };
    jest
      .spyOn(PanAndZoomImage, "default")
      .mockImplementationOnce(panZoomObject)
      .mockImplementationOnce(panZoomObject);

    const onResetCallbackSpy = jest.fn();

    const component = render(
      <FileHashImage
        isError={false}
        fileHash="hash"
        orientation={Orientation.Horizontal}
        onResetCallback={onResetCallbackSpy}
      />
    );

    // there is one problem with this test, is assumes the default value
    component.queryByRole("button")?.click();

    const img = component.queryByRole("img") as HTMLImageElement;
    expect(img.src).toBe(
      "http://localhost" +
        new UrlQuery().UrlThumbnailImageLargeOrExtraLarge("hash", true)
    );

    expect(onResetCallbackSpy).toBeCalled();
  });
});
