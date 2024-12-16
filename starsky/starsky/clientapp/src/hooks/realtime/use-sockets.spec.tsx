import { act, RenderResult } from "@testing-library/react";
import * as DifferenceInDate from "../../shared/date";
import { mountReactHook, MountReactHookResult } from "../___tests___/test-hook";
import * as useInterval from "../use-interval";
import { FakeWebSocketService } from "./___tests___/fake-web-socket-service";
import useSockets, { IUseSockets } from "./use-sockets";
import WebSocketService from "./websocket-service";
import * as WsCurrentStart from "./ws-current-start";

describe("useSockets", () => {
  let setupComponent: MountReactHookResult;
  let hook: IUseSockets;
  let component: RenderResult;

  function mountComponent() {
    setupComponent = mountReactHook(useSockets, []) as unknown as MountReactHookResult; // Mount a Component with our hook
    hook = setupComponent.componentHook as IUseSockets;
    component = setupComponent.componentMount;
  }

  it("default no error", () => {
    mountComponent();
    expect(hook.showSocketError).toBeFalsy();
    component.unmount();
  });

  it("feature toggle disabled", () => {
    const socketService = new WebSocketService("");
    localStorage.setItem("use-sockets", "false");

    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService);

    mountComponent();
    expect(hook.showSocketError).toBeFalsy();

    expect(wsCurrent).toHaveBeenCalledTimes(0);

    localStorage.setItem("use-sockets", "true");
    component.unmount();
  });

  it("ws current has been called", () => {
    const socketService = new WebSocketService("");
    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService);
    mountComponent();

    expect(wsCurrent).toHaveBeenCalled();
    expect(wsCurrent).toHaveBeenCalledTimes(1);
    expect(wsCurrent).toHaveBeenCalledWith(
      false,
      expect.any(Function),
      { current: true },
      expect.any(Function),
      expect.any(Function),
      expect.any(String),
      expect.any(Function)
    );

    wsCurrent.mockReset();
    component.unmount();
  });

  it("test retry when no response", () => {
    jest.useFakeTimers();
    const socketService = new FakeWebSocketService();

    // set the difference in time longer than 0.5 minutes
    jest.spyOn(DifferenceInDate, "DifferenceInDate").mockImplementationOnce(() => 1);

    jest.spyOn(WsCurrentStart, "default").mockClear();

    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockImplementationOnce(() => socketService)
      .mockImplementationOnce(() => socketService);

    mountComponent();

    act(() => {
      jest.advanceTimersByTime(40000);
    });

    expect(wsCurrent).toHaveBeenCalled();
    expect(wsCurrent).toHaveBeenCalledTimes(2);
    expect(wsCurrent).toHaveBeenCalledWith(
      false,
      expect.any(Function),
      { current: true },
      expect.any(Function),
      expect.any(Function),
      expect.any(String),
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
      .mockImplementationOnce((props) => props())
      .mockImplementationOnce((props) => props())
      .mockImplementationOnce(() => {});

    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockReset()
      .mockImplementationOnce(() => socketService)
      .mockImplementationOnce(() => socketService);

    mountComponent();

    act(() => {
      hook.setShowSocketError(null);
    });

    expect(wsCurrent).toHaveBeenCalled();
    expect(wsCurrent).toHaveBeenCalledTimes(2);

    expect(hook.showSocketError).toBeNull();

    component.unmount();
  });

  it("should ignore when Client is disabled", () => {
    jest.useFakeTimers();

    const socketService = new FakeWebSocketService();

    localStorage.setItem("use-sockets", "false");
    const wsCurrent = jest
      .spyOn(WsCurrentStart, "default")
      .mockReset()
      .mockImplementationOnce(() => socketService);

    mountComponent();

    expect(wsCurrent).toHaveBeenCalledTimes(0);

    localStorage.removeItem("use-sockets");
    jest.spyOn(WsCurrentStart, "default").mockReset();
    component.unmount();
    jest.useRealTimers();
  });
});
