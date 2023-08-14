import {
  act,
  createEvent,
  fireEvent,
  render,
  screen
} from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useInterval from "../../../hooks/use-interval";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import * as Modal from "../../atoms/modal/modal";
import ModalPublish from "./modal-publish";

describe("ModalPublish", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );
  });

  it("Publish button exist test", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: null
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("publish")).toBeTruthy();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Publish flow with default options -> and waiting", async () => {
    jest
      .spyOn(useInterval, "default")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: [{ key: "key", value: true }]
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const connectionDefault: IConnectionDefault = {
      statusCode: 200,
      data: [{ key: "key", value: true }]
    };
    const mockIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault);
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    const formControls = screen
      .queryAllByTestId("form-control")
      .find((p) => p.getAttribute("data-name") === "item-name");
    const tags = formControls as HTMLElement[][0];
    expect(tags).not.toBe(undefined);

    // update component + now press a key
    tags.textContent = "a";
    const inputEvent = createEvent.input(tags, { key: "a" });
    fireEvent(tags, inputEvent);

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("publish")).toBeTruthy();

    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const connectionDefault2: IConnectionDefault = {
      statusCode: 206,
      data: ["key"]
    };
    const mockIConnectionDefault2: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault2);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault2);

    await act(async () => {
      await screen.queryByTestId("publish")?.click();
    });

    expect(screen.queryByTestId("modal-publish-subheader")?.textContent).toBe(
      "One moment please"
    );

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Preflight check fails and gives error", async () => {
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: [{ key: "failed-key", value: false }]
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    expect(screen.getByTestId("publish-profile-preflight-error")).toBeTruthy();
    expect(
      screen.getByTestId("publish-profile-preflight-error").innerHTML
    ).toContain("failed-key");

    expect(useFetchSpy).toBeCalled();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Preflight check succeeds and gives no error", async () => {
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: [{ key: "succeed-key", value: true }]
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    expect(screen.queryByTestId("publish-profile-preflight-error")).toBeFalsy();

    expect(useFetchSpy).toBeCalled();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Fail - Publish flow with default options -> and waiting 2", async () => {
    const connectionDefault: IConnectionDefault = {
      statusCode: 500,
      data: null
    };
    const mockIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault);

    jest
      .spyOn(useInterval, "default")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: [{ key: "key", value: true }]
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault)
      .mockImplementationOnce(() => mockIConnectionDefault)
      .mockImplementationOnce(() => mockIConnectionDefault);

    const formControls = screen
      .queryAllByTestId("form-control")
      .find((p) => p.getAttribute("data-name") === "item-name");
    const tags = formControls as HTMLElement[][0];
    expect(tags).not.toBe(undefined);

    // update component + now press a key
    tags.textContent = "a";
    const inputEvent = createEvent.input(tags, { key: "a" });
    fireEvent(tags, inputEvent);

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("publish")).toBeTruthy();

    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await act(async () => {
      await screen.getByTestId("publish")?.click();
    });

    expect(screen.getByTestId("modal-publish-content-text")?.textContent).toBe(
      "Something went wrong with exporting Retry this"
    );

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Fail - Publish flow with default options -> wait and go back", async () => {
    const connectionDefault: IConnectionDefault = {
      statusCode: 500,
      data: null
    };
    const mockIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault);

    jest
      .spyOn(useInterval, "default")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: ["_default"]
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault)
      .mockImplementationOnce(() => mockIConnectionDefault)
      .mockImplementationOnce(() => mockIConnectionDefault);

    const formControls = screen
      .queryAllByTestId("form-control")
      .find((p) => p.getAttribute("data-name") === "item-name");
    const tags = formControls as HTMLElement[][0];
    expect(tags).not.toBe(undefined);

    // update component + now press a key
    tags.textContent = "a";
    const inputEvent = createEvent.input(tags, { key: "a" });
    fireEvent(tags, inputEvent);

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("publish")).toBeTruthy();

    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await act(async () => {
      await screen.getByTestId("publish")?.click();
    });

    // and go back to
    await act(async () => {
      await screen.queryByTestId("publish-retry-export-fail")?.click();
    });

    // and be at the main window
    expect(screen.getByTestId("publish")).not.toBeNull();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("test if handleExit is called", () => {
    // callback
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
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const handleExitSpy = jest.fn();

    const modal = render(
      <ModalPublish select={["/"]} isOpen={true} handleExit={handleExitSpy} />
    );

    expect(handleExitSpy).toBeCalled();

    // and clean afterwards
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    act(() => {
      modal.unmount();
    });
  });

  it("undo typing", async () => {
    console.log("undo typing");

    jest.spyOn(Modal, "default").mockRestore();

    const mockGetIConnectionDefault = {
      statusCode: 200,
      data: ["_default"]
    } as IConnectionDefault;

    jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockReset()
      .mockImplementationOnce(() => {
        return Promise.resolve({
          statusCode: 200,
          data: true
        } as IConnectionDefault);
      });

    const modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    const formControls = screen
      .queryAllByTestId("form-control")
      .find((p) => p.getAttribute("data-name") === "item-name");
    const tags = formControls as HTMLElement[][0];
    expect(tags).not.toBe(undefined);

    // update component + now press a key
    await act(async () => {
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      await fireEvent(tags, inputEvent);
    });

    expect(screen.getByTestId("modal-publish-warning-box")).toBeTruthy();

    // and now undo
    await act(async () => {
      tags.textContent = "";
      const inputEvent = createEvent.input(tags, { key: "" });
      await fireEvent(tags, inputEvent);
    });

    // should only send get once. the second time should be avoid due sending emthy string

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledTimes(1);

    modal.unmount();
  });
});
