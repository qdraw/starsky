import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Notification from "../../atoms/notification/notification";
import MenuOptionDesktopEditorOpenSingle, {
  OpenDesktopSingle
} from "./menu-option-desktop-editor-open-single";

describe("MenuOptionDesktopEditorOpenSingle", () => {
  it("should render without errors", () => {
    render(<MenuOptionDesktopEditorOpenSingle subPath="" collections={false} isReadOnly={true} />);
    // You can add more specific assertions about the rendered output if needed
  });

  it("should call OpenDesktopSingle when MenuOption is clicked", () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: true
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: true,
      statusCode: 200
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefaultResolve);

    const subPath = "/test.jpg";
    const collections = true;
    const isReadOnly = false;

    const container = render(
      <MenuOptionDesktopEditorOpenSingle
        subPath={subPath}
        collections={collections}
        isReadOnly={isReadOnly}
      />
    );

    expect(useFetchSpy).toHaveBeenCalled();

    fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open-single"));

    expect(fetchPostSpy).toHaveBeenCalled();
    expect(fetchPostSpy).toHaveBeenNthCalledWith(
      1,
      new UrlQuery().UrlApiDesktopEditorOpen(),
      "f=%2Ftest.jpg&collections=true"
    );

    container.unmount();
  });

  it("calls StartMenuOptionDesktopEditorOpenSelection on hotkey trigger", async () => {
    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: true,
      statusCode: 200
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefaultResolve)
      .mockImplementationOnce(() => mockIConnectionDefaultResolve);

    const container = render(
      <MenuOptionDesktopEditorOpenSingle
        subPath={"/test.jpg"}
        collections={false}
        isReadOnly={false}
      />
    );

    fireEvent.keyDown(document.body, { key: "e", ctrlKey: true });

    await waitFor(() => {
      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);

      expect(fetchPostSpy).toHaveBeenNthCalledWith(
        1,
        new UrlQuery().UrlApiDesktopEditorOpen(),
        "f=%2Ftest.jpg&collections=false"
      );
      container.unmount();
    });
  });

  it("feature toggle disabled", () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: false
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const subPath = "/test.jpg";
    const collections = true;
    const isReadOnly = false;

    const container = render(
      <MenuOptionDesktopEditorOpenSingle
        subPath={subPath}
        collections={collections}
        isReadOnly={isReadOnly}
      />
    );

    waitFor(() => {
      expect(useFetchSpy).toHaveBeenCalled();

      expect(screen.queryByTestId("menu-option-desktop-editor-open-single")).toBeFalsy();
    });

    container.unmount();
  });

  it("should hide feature toggle - set Error", async () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: true
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce(() => <></>);

    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 500
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefaultResolve);

    const subPath = "/test.jpg";
    const collections = true;
    const isReadOnly = false;

    const container = render(
      <MenuOptionDesktopEditorOpenSingle
        subPath={subPath}
        collections={collections}
        isReadOnly={isReadOnly}
      />
    );

    expect(useFetchSpy).toHaveBeenCalled();

    fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open-single"));

    expect(fetchPostSpy).toHaveBeenCalled();
    expect(fetchPostSpy).toHaveBeenNthCalledWith(
      1,
      new UrlQuery().UrlApiDesktopEditorOpen(),
      "f=%2Ftest.jpg&collections=true"
    );

    await waitFor(() => {
      expect(notificationSpy).toHaveBeenCalled();
    });
    container.unmount();
  });

  it("OpenDesktopSingle readonly should skip", async () => {
    const result = await OpenDesktopSingle("/", false, jest.fn(), "error", true);
    expect(result).toBeFalsy();
  });
});
