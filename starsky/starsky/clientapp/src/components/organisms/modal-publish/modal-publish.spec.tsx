import { act, createEvent, fireEvent, render } from "@testing-library/react";
import React from "react";
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
    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    expect(useFetchSpy).toBeCalled();
    expect(modal.queryByTestId("publish")).toBeTruthy();

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
      data: ["_default"]
    } as IConnectionDefault;
    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var connectionDefault: IConnectionDefault = {
      statusCode: 200,
      data: "key"
    };
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      connectionDefault
    );
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var modal = render(
      <ModalPublish
        select={["/"]}
        isOpen={true}
        handleExit={() => {}}
      ></ModalPublish>
    );

    const formControls = modal
      .queryAllByTestId("form-control")
      .find((p) => p.getAttribute("data-name") === "item-name");
    const tags = formControls as HTMLElement[][0];
    expect(tags).not.toBe(undefined);

    // update component + now press a key
    act(() => {
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);
    });

    expect(useFetchSpy).toBeCalled();
    expect(modal.queryByTestId("publish")).toBeTruthy();

    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var connectionDefault2: IConnectionDefault = {
      statusCode: 206,
      data: "key"
    };
    const mockIConnectionDefault2: Promise<IConnectionDefault> = Promise.resolve(
      connectionDefault2
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault2);

    await act(async () => {
      await modal.queryByTestId("publish")?.click();
    });

    expect(modal.queryByTestId("modal-publish-subheader")?.textContent).toBe(
      "One moment please"
    );

    // and clean afterwards
    act(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });
  });

  it("Fail - Publish flow with default options -> and waiting 2", async () => {
    var connectionDefault: IConnectionDefault = { statusCode: 500, data: null };
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      connectionDefault
    );

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
    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var modal = render(
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

    // update component + now press a key
    modal.find('[data-name="item-name"]').getDOMNode().textContent = "a";
    modal.find('[data-name="item-name"]').simulate("input", { key: "a" });

    expect(useFetchSpy).toBeCalled();
    expect(modal.exists('[data-test="publish"]')).toBeTruthy();

    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await act(async () => {
      await modal.find('[data-test="publish"]').simulate("click");
    });

    expect(modal.find(".content--text").text()).toBe(
      "Something went wrong with exporting"
    );

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
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var handleExitSpy = jest.fn();

    var modal = render(
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
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
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

    act(() => {
      // update component + now press a key
      modal.find('[data-name="item-name"]').getDOMNode().textContent = "a";
      modal.find('[data-name="item-name"]').simulate("input", { key: "a" });
    });

    // need await for update
    await act(async () => {
      await modal.render();
    });

    expect(modal.html()).toContain('class="warning-box"');

    // and now undo
    act(() => {
      // update component + now press a key
      modal.find('[data-name="item-name"]').getDOMNode().textContent = "";
      modal.find('[data-name="item-name"]').simulate("input", { key: "" });
    });

    // should only send get once. the second time should be avoid due sending emthy string

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledTimes(1);
  });
});
