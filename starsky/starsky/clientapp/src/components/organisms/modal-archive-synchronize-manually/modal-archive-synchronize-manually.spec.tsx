import { act, render } from "@testing-library/react";
import { ReactWrapper } from "enzyme";
import React from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalArchiveSynchronizeManually from "./modal-archive-synchronize-manually";

describe("ModalArchiveSynchronizeManually", () => {
  it("renders", () => {
    render(
      <ModalArchiveSynchronizeManually
        isOpen={true}
        parentFolder="/"
        handleExit={() => {}}
      >
        test
      </ModalArchiveSynchronizeManually>
    );
  });

  describe("with Context", () => {
    describe("buttons exist", () => {
      var modal: ReactWrapper;
      beforeAll(() => {
        modal = render(
          <ModalArchiveSynchronizeManually
            parentFolder={"/"}
            isOpen={true}
            handleExit={() => {}}
          />
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
        expect(modal.exists('[data-test="force-sync"]')).toBeTruthy();
      });
      it("remove-cache", () => {
        expect(modal.exists('[data-test="remove-cache"]')).toBeTruthy();
      });
      it("geo-sync", () => {
        expect(modal.exists('[data-test="geo-sync"]')).toBeTruthy();
      });
      it("thumbnail-generation", () => {
        expect(modal.exists('[data-test="thumbnail-generation"]')).toBeTruthy();
      });
    });

    describe("click button", () => {
      var modal: ReactWrapper;
      beforeEach(() => {
        jest.useFakeTimers();
        modal = render(
          <ModalArchiveSynchronizeManually
            parentFolder={"/"}
            isOpen={true}
            handleExit={() => {}}
          />
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

      it("force-sync (only first get)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
          {
            statusCode: 200,
            data: null
          } as IConnectionDefault
        );

        var fetchPostSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        modal.find('[data-test="force-sync"]').simulate("click");

        expect(fetchPostSpy).toBeCalled();
        expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSync("/"), "");
      });

      it("remove-cache (only first get)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
          {
            statusCode: 200,
            data: null
          } as IConnectionDefault
        );

        var fetchGetSpy = jest
          .spyOn(FetchGet, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        act(() => {
          modal.find('[data-test="remove-cache"]').simulate("click");
        });

        expect(fetchGetSpy).toBeCalled();
        expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlRemoveCache("/"));

        fetchGetSpy.mockReset();
      });

      it("geo-sync (only first post)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
          {
            statusCode: 200,
            data: null
          } as IConnectionDefault
        );

        var fetchPostSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        var urlGeoSyncUrlQuerySpy = jest
          .spyOn(UrlQuery.prototype, "UrlGeoSync")
          .mockImplementationOnce(() => "");

        expect(modal.exists('[data-test="geo-sync"]')).toBeTruthy();

        modal.find('[data-test="geo-sync"]').simulate("click");
        expect(urlGeoSyncUrlQuerySpy).toBeCalled();
        expect(fetchPostSpy).toBeCalled();
      });

      it("remove-cache (only first POST)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
          {
            statusCode: 200,
            data: null
          } as IConnectionDefault
        );

        var fetchGetSpy = jest
          .spyOn(FetchPost, "default")
          .mockImplementationOnce(() => mockGetIConnectionDefault);

        act(() => {
          modal.find('[data-test="thumbnail-generation"]').simulate("click");
        });

        expect(fetchGetSpy).toBeCalled();
        expect(fetchGetSpy).toBeCalledWith(
          new UrlQuery().UrlThumbnailGeneration(),
          "f=%2F"
        );

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

      var handleExitSpy = jest.fn();

      var component = render(
        <ModalArchiveSynchronizeManually
          parentFolder="/"
          isOpen={true}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
