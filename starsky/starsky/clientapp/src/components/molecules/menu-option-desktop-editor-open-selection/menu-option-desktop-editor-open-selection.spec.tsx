import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType, newIRelativeObjects } from "../../../interfaces/IDetailView";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Notification from "../../atoms/notification/notification";
import * as ModalDesktopEditorOpenSelectionConfirmation from "../../organisms/modal-desktop-editor-open-selection-confirmation/modal-desktop-editor-open-selection-confirmation";
import MenuOptionDesktopEditorOpenSelection, {
  OpenDesktop,
  StartMenuOptionDesktopEditorOpenSelection
} from "./menu-option-desktop-editor-open-selection";

describe("ModalDesktopEditorOpenConfirmation", () => {
  const state = {
    fileIndexItems: [
      {
        filePath: "/file2.jpg",
        fileName: "file1.jpg",
        fileCollectionName: "file1",
        fileHash: "1",
        parentDirectory: "/",
        status: IExifStatus.Ok
      },
      {
        filePath: "/file2.jpg",
        fileName: "file2.jpg",
        fileCollectionName: "file1",
        fileHash: "1",
        parentDirectory: "/",
        status: IExifStatus.Ok
      }
    ],
    relativeObjects: newIRelativeObjects(),
    subPath: "",
    breadcrumb: [],
    colorClassUsage: [],
    collectionsCount: 1,
    colorClassActiveList: [],
    pageType: PageType.Archive,
    isReadOnly: false,
    dateCache: 1
  } as IArchiveProps;

  describe("default function", () => {
    it("renders without crashing", () => {
      render(
        <MenuOptionDesktopEditorOpenSelection
          isReadOnly={true}
          select={[]}
          state={state}
          setEnableMoreMenu={() => {}}
        />
      );
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

      const component = render(
        <MenuOptionDesktopEditorOpenSelection
          isReadOnly={true}
          select={["file1.jpg", "file2.jpg"]}
          state={state}
          setEnableMoreMenu={() => {}}
        />
      );
      fireEvent.keyDown(document.body, { key: "e", ctrlKey: true });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenNthCalledWith(
        1,
        new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker(),
        "f=%2Ffile1.jpg%3B%2Ffile2.jpg"
      );

      await waitFor(() => {
        expect(fetchPostSpy).toHaveBeenCalledTimes(2);
        expect(fetchPostSpy).toHaveBeenNthCalledWith(
          2,
          new UrlQuery().UrlApiDesktopEditorOpen(),
          "f=%2Ffile1.jpg%3B%2Ffile2.jpg&collections=true"
        );
        component.unmount();
      });
    });

    it("ModalDesktopEditorOpenSelectionConfirmation and open modal due FetchPost false", async () => {
      const mockGetIConnectionDefaultFeatureToggle = {
        statusCode: 200,
        data: {
          openEditorEnabled: true
        } as IEnvFeatures
      } as IConnectionDefault;

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: false,
        statusCode: 200
      } as IConnectionDefault);

      const modalSpy = jest
        .spyOn(ModalDesktopEditorOpenSelectionConfirmation, "default")
        .mockImplementationOnce(() => <></>);

      const useLocationFunction = () => {
        return {
          location: {
            search: "?f=test1.jpg"
          } as unknown as Location,
          navigate: jest.fn()
        };
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(useLocationFunction)
        .mockImplementationOnce(useLocationFunction);

      jest.spyOn(Notification, "default").mockImplementationOnce(() => <></>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      const component = render(
        <MenuOptionDesktopEditorOpenSelection
          state={state}
          select={["file1.jpg"]}
          isReadOnly={false}
        />
      );

      expect(useFetchSpy).toHaveBeenCalled();

      fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open"));

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenNthCalledWith(
        1,
        new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker(),
        "f=%2Ffile1.jpg"
      );

      await waitFor(() => {
        expect(modalSpy).toHaveBeenCalled();

        component.unmount();
      });
    });

    it("ModalDesktopEditorOpenSelectionConfirmation and FetchPost SearchPage is collections false", async () => {
      const mockGetIConnectionDefaultFeatureToggle = {
        statusCode: 200,
        data: {
          openEditorEnabled: true
        } as IEnvFeatures
      } as IConnectionDefault;

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 200
      } as IConnectionDefault);

      const useLocationFunction = () => {
        return {
          location: {
            search: "?f=test1.jpg&collections=true" // due search is false
          } as unknown as Location,
          navigate: jest.fn()
        };
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(useLocationFunction)
        .mockImplementationOnce(useLocationFunction);

      jest.spyOn(Notification, "default").mockImplementationOnce(() => <></>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      const component = render(
        <MenuOptionDesktopEditorOpenSelection
          state={{ ...state, pageType: PageType.Search }}
          select={["file1.jpg"]}
          isReadOnly={false}
        />
      );

      expect(useFetchSpy).toHaveBeenCalled();

      fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open"));

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenNthCalledWith(
        1,
        new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker(),
        "f=%2Ffile1.jpg"
      );

      await waitFor(() => {
        expect(fetchPostSpy).toHaveBeenCalledTimes(2);
        expect(fetchPostSpy).toHaveBeenNthCalledWith(
          2,
          new UrlQuery().UrlApiDesktopEditorOpen(),
          "f=%2Ffile1.jpg&collections=false"
        );
        component.unmount();
      });
    });

    it("ModalDesktopEditorOpenSelectionConfirmation and FetchPost error status Notification", async () => {
      const mockGetIConnectionDefaultFeatureToggle = {
        statusCode: 200,
        data: {
          openEditorEnabled: true
        } as IEnvFeatures
      } as IConnectionDefault;

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 200
      } as IConnectionDefault);

      const mockIConnectionDefaultErrorStatus: Promise<IConnectionDefault> = Promise.resolve({
        data: null, // FAIL:!
        statusCode: 500
      } as IConnectionDefault);

      const useLocationFunction = () => {
        return {
          location: {
            search: "?f=test1.jpg"
          } as unknown as Location,
          navigate: jest.fn()
        };
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(useLocationFunction)
        .mockImplementationOnce(useLocationFunction);

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <></>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultErrorStatus);

      const component = render(
        <MenuOptionDesktopEditorOpenSelection
          state={state}
          select={["file1.jpg"]}
          isReadOnly={false}
        />
      );

      expect(useFetchSpy).toHaveBeenCalled();

      fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open"));

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenNthCalledWith(
        1,
        new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker(),
        "f=%2Ffile1.jpg"
      );

      await waitFor(() => {
        expect(fetchPostSpy).toHaveBeenCalledTimes(2);
        expect(fetchPostSpy).toHaveBeenNthCalledWith(
          2,
          new UrlQuery().UrlApiDesktopEditorOpen(),
          "f=%2Ffile1.jpg&collections=true"
        );
        // Failure
        expect(notificationSpy).toHaveBeenCalled();

        component.unmount();
      });
    });

    it("ModalDesktopEditorOpenSelectionConfirmation and close due FetchPost true", async () => {
      const mockGetIConnectionDefaultFeatureToggle = {
        statusCode: 200,
        data: {
          openEditorEnabled: true
        } as IEnvFeatures
      } as IConnectionDefault;

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 200
      } as IConnectionDefault);

      const useLocationFunction = () => {
        return {
          location: window.location,
          navigate: jest.fn()
        };
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(useLocationFunction)
        .mockImplementationOnce(useLocationFunction);

      jest.spyOn(Notification, "default").mockImplementationOnce(() => <></>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
        .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      const component = render(
        <MenuOptionDesktopEditorOpenSelection
          state={state}
          select={["file1.jpg"]}
          isReadOnly={false}
        />
      );

      expect(useFetchSpy).toHaveBeenCalled();

      fireEvent.click(screen.getByTestId("menu-option-desktop-editor-open"));

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenLastCalledWith(
        new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker(),
        "f=%2Ffile1.jpg"
      );

      component.unmount();
    });
  });

  describe("StartMenuOptionDesktopEditorOpenSelection", () => {
    it("StartMenuOptionDesktopEditorOpenSelection emthy array returns false", async () => {
      const result = await StartMenuOptionDesktopEditorOpenSelection(
        [],
        false,
        state,
        jest.fn(),
        "",
        jest.fn(),
        false
      );
      expect(result).toBeFalsy();
    });

    it("readonly", async () => {
      const result = await StartMenuOptionDesktopEditorOpenSelection(
        ["test"],
        false,
        state,
        jest.fn(),
        "",
        jest.fn(),
        true
      );
      expect(result).toBeFalsy();
    });

    it("sets modal confirmation open files if openWithoutConformationResult is false", async () => {
      const select = ["file1.jpg", "file2.jpg"];
      const collections = false;

      const setIsError = jest.fn();
      const messageDesktopEditorUnableToOpen = "[for example] Unable to open desktop editor";
      const setModalConfirmationOpenFiles = jest.fn();

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: false,
        statusCode: 200
      } as IConnectionDefault);

      jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      await StartMenuOptionDesktopEditorOpenSelection(
        select,
        collections,
        state,
        setIsError,
        messageDesktopEditorUnableToOpen,
        setModalConfirmationOpenFiles,
        false
      );
      expect(setModalConfirmationOpenFiles).toHaveBeenCalledWith(true);
      expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
    });

    it("calls openDesktop if openWithoutConformationResult is true and open succceed", async () => {
      const select = ["file1.jpg", "file2.jpg"];
      const collections = false;
      const setIsError = jest.fn();
      const messageDesktopEditorUnableToOpen = "[for example] Unable to open desktop editor";
      const setModalConfirmationOpenFiles = jest.fn();
      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 200
      } as IConnectionDefault);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      await StartMenuOptionDesktopEditorOpenSelection(
        select,
        collections,
        state,
        setIsError,
        messageDesktopEditorUnableToOpen,
        setModalConfirmationOpenFiles,
        false
      );
      expect(setModalConfirmationOpenFiles).not.toHaveBeenCalled(); // Modal confirmation should not be set in this case
      expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
      expect(fetchPostSpy).toHaveBeenCalledTimes(2);
      expect(fetchPostSpy).toHaveBeenLastCalledWith(
        new UrlQuery().UrlApiDesktopEditorOpen(),
        "f=%2Ffile1.jpg%3B%2Ffile2.jpg&collections=false"
      );
    });

    it("calls openDesktop if openWithoutConformationResult is true but open failed", async () => {
      const select = ["file1.jpg", "file2.jpg"];
      const collections = false;
      const setIsError = jest.fn();
      const messageDesktopEditorUnableToOpen = "[for example] Unable to open desktop editor";
      const setModalConfirmationOpenFiles = jest.fn();
      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 200
      } as IConnectionDefault);

      const mockIConnectionDefaultFailed: Promise<IConnectionDefault> = Promise.resolve({
        data: true,
        statusCode: 300
      } as IConnectionDefault);

      jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefaultResolve)
        .mockImplementationOnce(() => mockIConnectionDefaultFailed);

      await StartMenuOptionDesktopEditorOpenSelection(
        select,
        collections,
        state,
        setIsError,
        messageDesktopEditorUnableToOpen,
        setModalConfirmationOpenFiles,
        false
      );
      expect(setModalConfirmationOpenFiles).not.toHaveBeenCalled(); // Modal confirmation should not be set in this case
      expect(setIsError).toHaveBeenCalled();
    });
  });

  describe("OpenDesktop", () => {
    it("open Desktop emthy array returns false", async () => {
      const result = await OpenDesktop([], false, state, jest.fn(), "");
      expect(result).toBeFalsy();
    });
  });
});
