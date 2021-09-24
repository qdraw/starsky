import { render } from "@testing-library/react";
import React from "react";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalForceDelete from "./modal-force-delete";

describe("ModalForceDelete", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalForceDelete
        isOpen={true}
        handleExit={() => {}}
        dispatch={jest.fn()}
        select={[]}
        setIsLoading={jest.fn()}
        setSelect={jest.fn()}
        state={newIArchive()}
      ></ModalForceDelete>
    );
  });

  it("should fetchPost and dispatch", async () => {
    const fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(async () => {
        return { statusCode: 200 } as IConnectionDefault;
      });

    const dispatch = jest.fn();
    const modal = render(
      <ModalForceDelete
        isOpen={true}
        handleExit={() => {}}
        dispatch={dispatch}
        select={["test.jpg"]}
        setIsLoading={jest.fn()}
        setSelect={jest.fn()}
        state={
          {
            fileIndexItems: [
              {
                parentDirectory: "/",
                filePath: "/test.jpg",
                fileName: "test.jpg"
              }
            ]
          } as IArchiveProps
        }
      ></ModalForceDelete>
    );

    expect(modal.exists('[data-test="force-delete"]')).toBeTruthy();
    expect(modal.exists("button.btn--default")).toBeTruthy();
    // need to await here
    await modal.find('[data-test="force-delete"]').simulate("click");
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith(
      new UrlQuery().UrlDeleteApi(),
      "f=%2Ftest.jpg&collections=false",
      "delete"
    );
    expect(dispatch).toBeCalled();
    modal.unmount();
  });

  it("should fetchPost and not dispatch due status error", async () => {
    const fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(async () => {
        return { statusCode: 500 } as IConnectionDefault;
      });

    const dispatch = jest.fn();
    const modal = render(
      <ModalForceDelete
        isOpen={true}
        handleExit={() => {}}
        dispatch={dispatch}
        select={["test.jpg"]}
        setIsLoading={jest.fn()}
        setSelect={jest.fn()}
        state={
          {
            fileIndexItems: [
              {
                parentDirectory: "/",
                filePath: "/test.jpg",
                fileName: "test.jpg"
              }
            ]
          } as IArchiveProps
        }
      ></ModalForceDelete>
    );

    expect(modal.exists('[data-test="force-delete"]')).toBeTruthy();
    expect(modal.exists("button.btn--default")).toBeTruthy();
    // need to await here
    await modal.find('[data-test="force-delete"]').simulate("click");
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith(
      new UrlQuery().UrlDeleteApi(),
      "f=%2Ftest.jpg&collections=false",
      "delete"
    );
    expect(dispatch).toBeCalledTimes(0);
    modal.unmount();
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
      <ModalForceDelete
        isOpen={true}
        dispatch={jest.fn()}
        select={[]}
        setIsLoading={jest.fn()}
        setSelect={jest.fn()}
        state={newIArchive()}
        handleExit={handleExitSpy}
      />
    );

    expect(handleExitSpy).toBeCalled();

    component.unmount();
  });
});
