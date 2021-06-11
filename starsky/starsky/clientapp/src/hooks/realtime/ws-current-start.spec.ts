import { useSocketsEventName } from "./use-sockets.const";
import WebSocketService from "./websocket-service";
import WsCurrentStart, {
  FireOnClose,
  FireOnError,
  FireOnMessage,
  FireOnOpen,
  HandleKeepAliveMessage,
  parseJson
} from "./ws-current-start";
import { FakeWebSocketService } from "./___tests___/fake-web-socket-service";

describe("WsCurrentStart", () => {
  let onOpenEvent = new Event("t");
  let onCloseEvent = new CloseEvent("t");
  const NewFakeWebSocketService = (): WebSocketService => {
    return new FakeWebSocketService(onOpenEvent, onCloseEvent);
  };

  describe("WsCurrentStart or default", () => {
    it("connected and returns default", () => {
      var setSocketConnectedSpy = jest.fn();
      var wsCurrentStart = WsCurrentStart(
        true,
        setSocketConnectedSpy,
        { current: true },
        jest.fn(),
        NewFakeWebSocketService
      ) as FakeWebSocketService;

      expect(setSocketConnectedSpy).toBeCalled();

      expect(wsCurrentStart.OnCloseCalled).toBeTruthy();
      expect(wsCurrentStart.OnErrorCalled).toBeTruthy();
      expect(wsCurrentStart.OnOpenCalled).toBeTruthy();
    });
  });

  describe("FireOnClose", () => {
    it("feature toggle disabled when statusCode is 1008", () => {
      var isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1008 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle disabled when statusCode is 1009", () => {
      var isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1009 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle enabled when statusCode is 1000", () => {
      var isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1000 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeTruthy();
    });
    it("check if setSocketConnected is called", () => {
      var setSocketConnectedSpy = jest.fn();
      FireOnClose(
        new CloseEvent("t", { code: 1000 }),
        true,
        setSocketConnectedSpy,
        { current: true }
      );
      expect(setSocketConnectedSpy).toBeCalled();
    });
  });

  describe("FireOnError", () => {
    it("check if setSocketConnected is called", () => {
      var setSocketConnectedSpy = jest.fn();
      FireOnError(true, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalled();
    });
  });

  describe("FireOnOpen", () => {
    it("check if setSocketConnected is called", () => {
      var setSocketConnectedSpy = jest.fn();
      FireOnOpen(false, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalled();
    });
    it("check if setSocketConnected is called 0 times when true", () => {
      var setSocketConnectedSpy = jest.fn();
      FireOnOpen(true, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalledTimes(0);
    });
  });

  describe("FireOnMessage", () => {
    it("check if setKeepAliveTimeSpy is on Welcome", () => {
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"welcome": true}' }),
        setKeepAliveTimeSpy
      );
      expect(setKeepAliveTimeSpy).toBeCalled();
    });

    it("check if setKeepAliveTimeSpy is on Time", () => {
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"time": 1}' }),
        setKeepAliveTimeSpy
      );
      expect(setKeepAliveTimeSpy).toBeCalled();
    });

    it("should ignore undefined data", () => {
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: undefined }),
        setKeepAliveTimeSpy
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should ignore invalid json data", () => {
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: "1{1\\" }),
        setKeepAliveTimeSpy
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should ignore keep alive when sending real message", () => {
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"data": 1}' }),
        setKeepAliveTimeSpy
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should fire an event", (done) => {
      document.body.addEventListener(useSocketsEventName, (e) => {
        var event = e as CustomEvent;
        expect(event.detail).toStrictEqual({ data: 1 });
        done();
      });

      FireOnMessage(new MessageEvent("t", { data: '{"data": 1}' }), jest.fn());
    });
  });

  describe("HandleKeepAliveMessage", () => {
    it("should ignore keep alive when sending real message", () => {
      var setKeepAliveTimeSpy = jest.fn();
      HandleKeepAliveMessage(setKeepAliveTimeSpy, { data: '{"data": 1}' });
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });
  });

  describe("parse Json", () => {
    it("should skip system message", () => {
      const result = parseJson("[system]");
      expect(result).toBeNull();
    });

    it("should skip invalid json", () => {
      console.error("next json error ->");
      const result = parseJson("['''''83]");
      expect(result).toBeNull();
    });

    it("should parse json", () => {
      const result = parseJson("83");
      expect(result).toBe(83);
    });
  });
});
