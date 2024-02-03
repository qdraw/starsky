import { render, screen } from "@testing-library/react";
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
    render(<FileHashImage fileHash={""} />);
  });

  beforeEach(() => {
    jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockImplementationOnce(() => Promise.resolve(true));
  });

  it("Rotation API is called return 202", async () => {
    console.log("-- Rotation API is called return 202 --");

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
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

    const detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve(false));

    const spyGet = jest
      .spyOn(FetchGet, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    const component = await render(
      <FileHashImage fileHash="hash" orientation={Orientation.Horizontal} />
    );
    expect(detectRotationSpy).toHaveBeenCalled();

    expect(spyGet).toHaveBeenCalledWith(new UrlQuery().UrlThumbnailJsonApi("hash"));

    // and clean up
    component.unmount();
  });

  it("Rotation API is called return 200", async () => {
    console.log("-- Rotation API is called return 200 --");

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
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

    const detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve(false));

    const spyGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const component = render(
      <FileHashImage fileHash="hash" orientation={Orientation.Horizontal} />
    );

    // need to await here
    await expect(detectRotationSpy).toHaveBeenCalled();

    expect(spyGet).toHaveBeenCalledWith(new UrlQuery().UrlThumbnailJsonApi("hash"));

    component.unmount();
  });

  it("should ignore when DetectAutomaticRotation is true", async () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200
    } as IConnectionDefault);

    const detectRotationSpy = jest
      .spyOn(DetectAutomaticRotation, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve(true));

    const spyGet = jest
      .spyOn(FetchGet, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const component = render(
      <FileHashImage fileHash="hash" orientation={Orientation.Horizontal} />
    );

    // need to await here
    await expect(detectRotationSpy).toHaveBeenCalled();

    expect(spyGet).toHaveBeenCalledTimes(0);

    spyGet.mockReset();
    component.unmount();
  });

  it("onWheelCallback should replace source image when event is returned", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

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
      <FileHashImage fileHash="hash" orientation={Orientation.Horizontal} id={"fallbackPath"} />
    );

    const button = screen.getAllByRole("button")[0] as HTMLButtonElement;
    const image = screen.getAllByRole("img")[0] as HTMLImageElement;

    act(() => {
      button.click();
    });

    expect(image.src).toBe(
      "http://localhost" + new UrlQuery().UrlThumbnailZoom("hash", "fallbackPath", 1)
    );

    component.unmount();
  });

  it("onWheelCallback should return callback", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

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
        fileHash="hash"
        id="/test.jpg"
        orientation={Orientation.Horizontal}
        onWheelCallback={onWheelCallbackSpy}
      />
    );

    const button = screen.getAllByRole("button")[0];

    act(() => {
      button.click();
    });

    expect(onWheelCallbackSpy).toHaveBeenCalled();

    component.unmount();
  });

  it("with onResetCallback it should set UrlThumbnailImage", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

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
      <FileHashImage fileHash="hash" id="/test.jpg" orientation={Orientation.Horizontal} />
    );

    // there is one problem with this test, is assumes the default value
    screen.queryByRole("button")?.click();

    const img = screen.queryByRole("img") as HTMLImageElement;
    expect(img.src).toBe(
      "http://localhost" +
        new UrlQuery().UrlThumbnailImageLargeOrExtraLarge("hash", "/test.jpg", true)
    );

    component.unmount();
  });

  it("with onResetCallback it should set UrlThumbnailImage and pass callback", () => {
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

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
        fileHash="hash"
        id="/test.jpg"
        orientation={Orientation.Horizontal}
        onResetCallback={onResetCallbackSpy}
      />
    );

    // there is one problem with this test, is assumes the default value
    screen.queryByRole("button")?.click();

    const img = screen.queryByRole("img") as HTMLImageElement;
    expect(img.src).toBe(
      "http://localhost" +
        new UrlQuery().UrlThumbnailImageLargeOrExtraLarge("hash", "/test.jpg", true)
    );

    expect(onResetCallbackSpy).toHaveBeenCalled();

    component.unmount();
  });
});
