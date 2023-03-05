import { IConnectionDefault } from "../../interfaces/IConnectionDefault";
import * as FetchGet from "../../shared/fetch-get";
import { useSocketsEventName } from "./use-sockets.const";
import WebSocketService from "./websocket-service";
import WsCurrentStart, {
  FireOnClose,
  FireOnError,
  FireOnMessage,
  FireOnOpen,
  HandleKeepAliveMessage,
  HandleKeepAliveServerMessage,
  isKeepAliveMessage,
  parseJson,
  RestoreDataOnOpen
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
      const setSocketConnectedSpy = jest.fn();
      const wsCurrentStart = WsCurrentStart(
        true,
        setSocketConnectedSpy,
        { current: true },
        jest.fn(),
        NewFakeWebSocketService,
        "",
        jest.fn() as any
      ) as FakeWebSocketService;

      expect(setSocketConnectedSpy).toBeCalled();

      expect(wsCurrentStart.OnCloseCalled).toBeTruthy();
      expect(wsCurrentStart.OnErrorCalled).toBeTruthy();
      expect(wsCurrentStart.OnOpenCalled).toBeTruthy();
    });
  });

  describe("FireOnClose", () => {
    it("feature toggle disabled when statusCode is 1008", () => {
      const isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1008 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle disabled when statusCode is 1009", () => {
      const isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1009 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle enabled when statusCode is 1000", () => {
      const isEnabled = { current: true };
      FireOnClose(
        new CloseEvent("t", { code: 1000 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeTruthy();
    });
    it("check if setSocketConnected is called", () => {
      const setSocketConnectedSpy = jest.fn();
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
      const setSocketConnectedSpy = jest.fn();
      FireOnError(true, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalled();
    });
  });

  describe("FireOnOpen", () => {
    it("check if setSocketConnected is called", () => {
      const setSocketConnectedSpy = jest.fn();
      FireOnOpen(false, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalled();
    });
    it("check if setSocketConnected is called 0 times when true", () => {
      const setSocketConnectedSpy = jest.fn();
      FireOnOpen(true, setSocketConnectedSpy);
      expect(setSocketConnectedSpy).toBeCalledTimes(0);
    });
  });

  describe("isKeepAliveMessage", () => {
    it("isKeepAliveMessage false", () => {
      expect(isKeepAliveMessage({})).toBeFalsy();
    });

    it("isKeepAliveMessage welcome true", () => {
      expect(isKeepAliveMessage({ type: "Welcome" })).toBeTruthy();
    });

    it("isKeepAliveMessage Heartbeat true", () => {
      expect(isKeepAliveMessage({ type: "Heartbeat" })).toBeTruthy();
    });
  });

  describe("FireOnMessage", () => {
    it("check if setKeepAliveTimeSpy is on Welcome", () => {
      const setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", {
          data: '{"type" : "Welcome", "welcome": true}'
        }),
        setKeepAliveTimeSpy,
        jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalled();
    });

    it("check if setKeepAliveTimeSpy is on Time", () => {
      const setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"type" : "Welcome", "time": 1}' }),
        setKeepAliveTimeSpy,
        jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalled();
    });

    it("should ignore undefined data", () => {
      const setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: undefined }),
        setKeepAliveTimeSpy,
        jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should ignore invalid json data", () => {
      const setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: "1{1\\" }),
        setKeepAliveTimeSpy,
        jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should ignore keep alive when sending real message", () => {
      const setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"data": 1}' }),
        setKeepAliveTimeSpy,
        jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should fire an event", (done) => {
      document.body.addEventListener(useSocketsEventName, (e) => {
        const event = e as CustomEvent;
        expect(event.detail).toStrictEqual({ data: 1 });
        done();
      });

      FireOnMessage(
        new MessageEvent("t", { data: '{"data": 1}' }),
        jest.fn(),
        jest.fn() as any
      );
    });
  });

  describe("HandleKeepAliveMessage", () => {
    it("should ignore keep alive when sending real message", () => {
      const setKeepAliveTimeSpy = jest.fn();
      HandleKeepAliveMessage(setKeepAliveTimeSpy, { data: '{"data": 1}' });
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });
  });

  describe("HandleKeepAliveServerMessage", () => {
    it("should ignore keep alive when sending real message", () => {
      const setKeepAliveServerTimeSpy = jest.fn();
      HandleKeepAliveServerMessage(setKeepAliveServerTimeSpy, {
        data: '{"data": 1}'
      });
      expect(setKeepAliveServerTimeSpy).toBeCalledTimes(0);
    });
    it("should trigger when message is valid", () => {
      const setKeepAliveServerTimeSpy = jest.fn();
      HandleKeepAliveServerMessage(setKeepAliveServerTimeSpy, {
        type: "Welcome",
        data: { dateTime: 1 }
      });
      expect(setKeepAliveServerTimeSpy).toBeCalledTimes(1);
    });
    it("should trigger when message has no welcome", () => {
      const setKeepAliveServerTimeSpy = jest.fn();
      HandleKeepAliveServerMessage(setKeepAliveServerTimeSpy, {
        data: { dateTime: 1 }
      }); // should have type
      expect(setKeepAliveServerTimeSpy).toBeCalledTimes(0);
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

  describe("RestoreDataOnOpen", () => {
    it("both null", async () => {
      const result = await RestoreDataOnOpen(false, "");
      expect(result).toBeFalsy();
    });
    it("time null", async () => {
      const result = await RestoreDataOnOpen(true, "");
      expect(result).toBeFalsy();
    });
    it("connected false", async () => {
      const result = await RestoreDataOnOpen(false, "any");
      expect(result).toBeFalsy();
    });

    it("fetch bad request", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 400,
          data: { type: "Welcome" }
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const result = await RestoreDataOnOpen(true, "any");

      expect(result).toBeFalsy();
      expect(spyGet).toBeCalled();
    });

    it("data not array", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: { type: "Welcome" }
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const result = await RestoreDataOnOpen(true, "any");

      expect(result).toBeFalsy();
      expect(spyGet).toBeCalled();
    });

    it("data does not contain content", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: [{ test: "test" }]
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const result = await RestoreDataOnOpen(true, "any");

      expect(result).toBeFalsy();
      expect(spyGet).toBeCalled();
    });

    it("should return ok", async () => {
      const bodyDispatchSpy = jest
        .spyOn(document.body, "dispatchEvent")
        .mockImplementationOnce(() => true);
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: [{ content: '{"test": true}' }]
        } as IConnectionDefault);

      const spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const result = await RestoreDataOnOpen(true, "any");

      expect(result).toBeTruthy();
      expect(spyGet).toBeCalled();
      expect(bodyDispatchSpy).toBeCalled();
    });
  });
});
