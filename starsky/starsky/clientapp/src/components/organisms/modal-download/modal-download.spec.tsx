import { render, screen } from "@testing-library/react";
import { act } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useInterval from "../../../hooks/use-interval";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
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
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    expect(useFetchSpy).toHaveBeenCalled();
    expect(screen.getByTestId("thumbnail")).toBeTruthy();
    expect(screen.getByTestId("original")).toBeTruthy();

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
    const mockFetchGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockFetchGetIConnectionDefault);

    const modal = render(
      <ModalDownload
        collections={false}
        select={["/file0", "/file1.jpg"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    const thumbnail = screen.queryByTestId("thumbnail");
    expect(thumbnail).toBeTruthy();
    thumbnail?.click();

    expect(fetchPostSpy).toHaveBeenCalled();

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
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalDownload
        collections={false}
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalDownload>
    );

    expect(useFetchSpy).toHaveBeenCalled();

    const btnTest = screen.queryByTestId("btn-test");
    const original = screen.queryByTestId("original");

    expect(btnTest).toBeNull();
    expect(original).not.toBeNull();

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
    jest.spyOn(useFetch, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

    const handleExitSpy = jest.fn();

    const modal = render(
      <ModalDownload collections={false} select={["/"]} isOpen={true} handleExit={handleExitSpy} />
    );

    expect(handleExitSpy).toHaveBeenCalled();

    // and clean afterwards
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    act(() => {
      modal.unmount();
    });
  });
});
