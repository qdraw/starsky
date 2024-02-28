import { act, createEvent, fireEvent, render, screen } from "@testing-library/react";
import React from "react";
import { IArchive, newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType } from "../../../interfaces/IDetailView";
import * as FetchGet from "../../../shared/fetch/fetch-get";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalArchiveMkdir from "./modal-archive-mkdir";

describe("ModalArchiveMkdir", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalArchiveMkdir
        dispatch={jest.fn()}
        state={{} as any}
        isOpen={true}
        handleExit={() => {}}
      />
    );
  });

  describe("mkdir", () => {
    it("to non valid dirname", async () => {
      const state = {} as IArchiveProps;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const modal = render(
        <ModalArchiveMkdir
          state={state}
          dispatch={jest.fn()}
          isOpen={true}
          handleExit={() => {}}
        ></ModalArchiveMkdir>
      );

      const button = screen.queryByTestId("modal-archive-mkdir-btn-default") as HTMLButtonElement;

      const submitButtonBefore = button.disabled;
      expect(submitButtonBefore).toBeTruthy();

      const directoryName = screen.queryByTestId("form-control") as HTMLInputElement;

      // update component + now press a key
      directoryName.textContent = "a";
      const inputEvent = createEvent.input(directoryName, { key: "a" });
      fireEvent(directoryName, inputEvent);

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(screen.getByTestId("modal-archive-mkdir-warning-box")).toBeTruthy();

      const submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeTruthy();

      // cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("submit mkdir", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200
      } as IConnectionDefault);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      // use ==> import * as FetchGet from '../shared/fetch-get';
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 202,
        data: {
          ...newIArchive(),
          fileIndexItems: [],
          pageType: PageType.Search
        } as IArchiveProps
      } as IConnectionDefault);

      const fetchGetSpy = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const state = {
        subPath: "/test"
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const modal = render(
        <ModalArchiveMkdir
          state={state}
          dispatch={contextValues.dispatch}
          isOpen={true}
          handleExit={() => {}}
        ></ModalArchiveMkdir>
      );

      const button = screen.queryByTestId("modal-archive-mkdir-btn-default") as HTMLButtonElement;

      const directoryName = screen.queryByTestId("form-control") as HTMLInputElement;

      directoryName.textContent = "new folder";
      const inputEvent = createEvent.input(directoryName, { key: "a" });
      fireEvent(directoryName, inputEvent);

      // await is needed
      await act(async () => {
        await button.click();
      });

      // to create new directory
      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlDiskMkdir(),
        "f=%2Ftest%2Fnew+folder"
      );

      // to get the update
      expect(fetchGetSpy).toHaveBeenCalled();
      expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlIndexServerApi({ f: "/test" }));

      // to update context
      expect(contextValues.dispatch).toHaveBeenCalled();
      expect(contextValues.dispatch).toHaveBeenCalledWith({
        payload: { fileIndexItems: [], pageType: "Search" },
        type: "force-reset"
      });

      // cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
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
        <ModalArchiveMkdir
          state={{} as any}
          isOpen={true}
          dispatch={jest.fn()}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toHaveBeenCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
