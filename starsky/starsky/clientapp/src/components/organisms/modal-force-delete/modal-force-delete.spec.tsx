import { render, screen } from "@testing-library/react";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
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
    const fetchSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(async () => {
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

    const forceDelete = screen.queryByTestId("force-delete");
    expect(forceDelete).toBeTruthy();
    // need to await here
    await forceDelete?.click();

    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlDeleteApi(),
      "f=%2Ftest.jpg&collections=false",
      "delete"
    );
    expect(dispatch).toHaveBeenCalled();
    modal.unmount();
  });

  it("should fetchPost and not dispatch due status error", async () => {
    const fetchSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(async () => {
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

    const forceDelete = screen.queryByTestId("force-delete");
    expect(forceDelete).toBeTruthy();
    // need to await here
    await forceDelete?.click();

    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlDeleteApi(),
      "f=%2Ftest.jpg&collections=false",
      "delete"
    );
    expect(dispatch).toHaveBeenCalledTimes(0);
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

    expect(handleExitSpy).toHaveBeenCalled();

    component.unmount();
  });
});
