import { act, render, RenderResult, waitFor } from "@testing-library/react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalArchiveSynchronizeManually from "./modal-archive-synchronize-manually";

describe("ModalArchiveSynchronizeManually", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalArchiveSynchronizeManually isOpen={true} parentFolder="/" handleExit={() => {}} />
    );
  });

  describe("with Context", () => {
    describe("buttons exist", () => {
      let modal: RenderResult;
      beforeEach(() => {
        modal = render(
          <ModalArchiveSynchronizeManually parentFolder={"/"} isOpen={true} handleExit={() => {}} />
        );
      });

      afterAll(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
          modal.unmount();
        });
      });

      it("force-sync", () => {
        expect(modal.queryByTestId("force-sync")).toBeTruthy();
      });
      it("remove-cache", () => {
        expect(modal.queryByTestId("remove-cache")).toBeTruthy();
      });
      it("geo-sync", () => {
        expect(modal.queryByTestId("geo-sync")).toBeTruthy();
      });
      it("thumbnail-generation", () => {
        expect(modal.queryByTestId("thumbnail-generation")).toBeTruthy();
      });
    });

    describe("click button", () => {
      let modal: RenderResult;
      beforeEach(() => {
        jest.useFakeTimers();
        modal = render(
          <ModalArchiveSynchronizeManually parentFolder={"/"} isOpen={true} handleExit={() => {}} />
        );
      });

      afterEach(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
          modal.unmount();
        });
        jest.useRealTimers();
      });

      it("force-sync (only first get)", async () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200,
          data: null
        } as IConnectionDefault);

        const fetchPostSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        const forceSync = modal.queryByTestId("force-sync");
        expect(forceSync).toBeTruthy();

        await act(async () => {
          await forceSync?.click();
        });

        await waitFor(() => expect(fetchPostSpy).toHaveBeenCalled());
        expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().UrlSync("/"), "");
      });

      it("remove-cache (only first get)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200,
          data: null
        } as IConnectionDefault);

        const fetchGetSpy = jest
          .spyOn(FetchGet, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        const removeCache = modal.queryByTestId("remove-cache");
        expect(removeCache).toBeTruthy();

        act(() => {
          removeCache?.click();
        });

        expect(fetchGetSpy).toHaveBeenCalled();
        expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlRemoveCache("/"));

        fetchGetSpy.mockReset();
      });

      it("geo-sync (only first post)", async () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200,
          data: null
        } as IConnectionDefault);

        const fetchPostSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        const urlGeoSyncUrlQuerySpy = jest
          .spyOn(UrlQuery.prototype, "UrlGeoSync")
          .mockImplementationOnce(() => "");

        const geoSyncButton = modal.queryByTestId("geo-sync");
        expect(geoSyncButton).toBeTruthy();

        await act(async () => {
          await geoSyncButton?.click();
        });

        expect(urlGeoSyncUrlQuerySpy).toHaveBeenCalled();
        expect(fetchPostSpy).toHaveBeenCalled();
      });

      it("remove-cache (only first POST)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200,
          data: null
        } as IConnectionDefault);

        const fetchGetSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        const thumbnailGeneration = modal.queryByTestId("thumbnail-generation");
        expect(thumbnailGeneration).toBeTruthy();

        act(() => {
          thumbnailGeneration?.click();
        });

        expect(fetchGetSpy).toHaveBeenCalled();
        expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlThumbnailGeneration(), "f=%2F");

        fetchGetSpy.mockReset();
      });
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      const handleExitSpy = jest.fn();

      const component = render(
        <ModalArchiveSynchronizeManually
          parentFolder="/"
          isOpen={true}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toHaveBeenCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
