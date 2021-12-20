import { act, render } from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useInterval from "../../../hooks/use-interval";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import * as Modal from "../../atoms/modal/modal";
import ModalDownload from "./modal-download";

describe("ModalDownload", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    // interface IModalExportProps {
    //   isOpen: boolean;
    //   select: Array<string> | undefined;
    //   handleExit: Function;
    // }
    render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );
  });

  beforeEach(() => {});

  it("Single File", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: null
    } as IConnectionDefault;
    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var modal = render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    expect(useFetchSpy).toBeCalled();
    expect(modal.queryByTestId("thumbnail")).toBeTruthy();
    expect(modal.queryByTestId("orginal")).toBeTruthy();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Multiple Files -> click download", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: null
    } as IConnectionDefault;

    jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    jest
      .spyOn(useInterval, "default")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockFetchGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 200,
        data: null
      } as IConnectionDefault);

    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockFetchGetIConnectionDefault);

    var modal = render(
      <ModalDownload
        collections={false}
        select={["/file0", "/file1.jpg"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    const thumbnail = modal.queryByTestId("thumbnail");
    expect(thumbnail).toBeTruthy();
    thumbnail?.click();

    expect(fetchPostSpy).toBeCalled();

    // and clean afterwards
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    act(() => {
      modal.unmount();
    });
  });

  it("file type not supported", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 415,
      data: null
    } as IConnectionDefault;
    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var modal = render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    expect(useFetchSpy).toBeCalled();

    const btnTest = modal.queryByTestId("btn-test");
    const orginal = modal.queryByTestId("orginal");

    expect(btnTest).toBeNull();
    expect(orginal).not.toBeNull();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("test if handleExit is called", () => {
    // simulate if a user press on close
    // use as ==> import * as Modal from './modal';
    jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      props.handleExit();
      return <>{props.children}</>;
    });

    jest.spyOn(useInterval, "default").mockImplementationOnce(() => {});

    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 415,
      data: null
    } as IConnectionDefault;
    jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var handleExitSpy = jest.fn();

    var modal = render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={handleExitSpy}
      />
    );

    expect(handleExitSpy).toBeCalled();

    // and clean afterwards
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    act(() => {
      modal.unmount();
    });
  });
});
