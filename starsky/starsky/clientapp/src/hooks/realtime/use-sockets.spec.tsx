import { act } from "@testing-library/react";
import { ReactWrapper } from "enzyme";
import * as DifferenceInDate from "../../shared/date";
import * as useInterval from "../use-interval";
import { mountReactHook } from "../___tests___/test-hook";
import useSockets, { IUseSockets } from "./use-sockets";
import WebSocketService from "./websocket-service";
import * as WsCurrentStart from "./ws-current-start";
import { FakeWebSocketService } from "./___tests___/fake-web-socket-service";

describe("useSockets", () => {
  let setupComponent: any;
  let hook: IUseSockets;
  let component: ReactWrapper;

  function mountComponent() {
    setupComponent = mountReactHook(useSockets, []); // Mount a Component with our hook
    hook = setupComponent.componentHook as IUseSockets;
    component = setupComponent.componentMount;
  }

  it("default no error", () => {
    mountComponent();
    expect(hook.showSocketError).toBeFalsy();
    component.unmount();
  });

  it("feature toggle disabled", () => {
    var socketService = new WebSocketService("");
    localStorage.setItem("use-sockets", "false");

    var wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService);

    mountComponent();
    expect(hook.showSocketError).toBeFalsy();

    expect(wsCurrent).toBeCalledTimes(0);

    localStorage.setItem("use-sockets", "true");
    component.unmount();
  });

  it("ws current has been called", () => {
    var socketService = new WebSocketService("");
    var wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService);
    mountComponent();

    expect(wsCurrent).toBeCalled();
    expect(wsCurrent).toBeCalledTimes(1);
    expect(wsCurrent).toBeCalledWith(
      false,
      expect.any(Function),
      { current: true },
      expect.any(Function),
      expect.any(Function)
    );

    wsCurrent.mockReset();
    component.unmount();
  });

  it("test retry when no response", () => {
    (window as any).appInsights = jest.fn();
    (window as any).appInsights.trackTrace = jest.fn();

    jest.useFakeTimers();
    var socketService = new FakeWebSocketService();

    // set the difference in time longer than 0.5 minutes
    jest
      .spyOn(DifferenceInDate, "DifferenceInDate")
      .mockImplementationOnce(() => 1);

    jest.spyOn(WsCurrentStart, "default").mockClear();

    var wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService)
      .mockImplementationOnce(() => socketService);

    mountComponent();

    act(() => {
      jest.advanceTimersByTime(40000);
    });

    expect(wsCurrent).toBeCalled();
    expect(wsCurrent).toBeCalledTimes(2);
    expect(wsCurrent).toBeCalledWith(
      false,
      expect.any(Function),
      { current: true },
      expect.any(Function),
      expect.any(Function)
    );

    component.unmount();
    jest.useRealTimers();
  });

  it("should retry with null setShowSocketError", () => {
    const socketService = new FakeWebSocketService();

    jest
      .spyOn(DifferenceInDate, "DifferenceInDate")
      .mockImplementationOnce(() => 600)
      .mockImplementationOnce(() => 600);

    jest
      .spyOn(useInterval, "default")
      .mockImplementationOnce((props) => {
        props();
      })
      .mockImplementationOnce((props) => {
        props();
      })
      .mockImplementationOnce(() => {});

    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService)
      .mockImplementationOnce(() => socketService);

    mountComponent();

    act(() => {
      hook.setShowSocketError(null);
    });

    expect(wsCurrent).toBeCalled();
    expect(wsCurrent).toBeCalledTimes(2);

    expect(hook.showSocketError).toBeNull();

    component.unmount();
  });

  it("should ignore when Client is disabled", () => {
    jest.useFakeTimers();

    var socketService = new FakeWebSocketService();

    localStorage.setItem("use-sockets", "false");
    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService);

    mountComponent();

    expect(wsCurrent).toBeCalledTimes(0);

    localStorage.removeItem("use-sockets");
    jest.spyOn(WsCurrentStart, "default").mockReset();
    component.unmount();
    jest.useRealTimers();
  });
});
