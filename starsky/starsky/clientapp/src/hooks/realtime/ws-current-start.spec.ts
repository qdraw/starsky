import { useSocketsEventName } from "./use-sockets.const";
import WebSocketService from "./websocket-service";
import WsCurrentStart, {
	FireOnClose,
	FireOnError,
	FireOnMessage,
	FireOnOpen,
	HandleKeepAliveMessage,
	isKeepAliveMessage,
	parseJson, RestoreDataOnOpen
} from "./ws-current-start";
import { FakeWebSocketService } from "./___tests___/fake-web-socket-service";
import {IConnectionDefault} from "../../interfaces/IConnectionDefault";
import {IDetailView, PageType} from "../../interfaces/IDetailView";
import {Orientation} from "../../interfaces/IFileIndexItem";
import {IExifStatus} from "../../interfaces/IExifStatus";
import * as FetchGet from "../../shared/fetch-get";

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
			const isEnabled = {current: true};
			FireOnClose(
        new CloseEvent("t", { code: 1008 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle disabled when statusCode is 1009", () => {
			const isEnabled = {current: true};
			FireOnClose(
        new CloseEvent("t", { code: 1009 }),
        true,
        jest.fn(),
        isEnabled
      );
      expect(isEnabled.current).toBeFalsy();
    });
    it("feature toggle enabled when statusCode is 1000", () => {
			const isEnabled = {current: true};
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
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", {
          data: '{"type" : "Welcome", "welcome": true}'
        }),
        setKeepAliveTimeSpy,
				jest.fn() as any,
				
      );
      expect(setKeepAliveTimeSpy).toBeCalled();
    });

    it("check if setKeepAliveTimeSpy is on Time", () => {
      var setKeepAliveTimeSpy = jest.fn();
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
      var setKeepAliveTimeSpy = jest.fn();
      FireOnMessage(
        new MessageEvent("t", { data: '{"data": 1}' }),
        setKeepAliveTimeSpy,
				jest.fn() as any
      );
      expect(setKeepAliveTimeSpy).toBeCalledTimes(0);
    });

    it("should fire an event", (done) => {
      document.body.addEventListener(useSocketsEventName, (e) => {
        var event = e as CustomEvent;
        expect(event.detail).toStrictEqual({ data: 1 });
        done();
      });

      FireOnMessage(new MessageEvent("t", { data: '{"data": 1}' }), jest.fn(), jest.fn() as any);
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
					data: {type: "Welcome"}
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
					data: {type: "Welcome"}
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
					data: {type: "Welcome", data: [{"test": "test"}]}
				} as IConnectionDefault);
			const spyGet = jest
				.spyOn(FetchGet, "default")
				.mockImplementationOnce(() => mockGetIConnectionDefault);

			const result = await RestoreDataOnOpen(true, "any");

			expect(result).toBeFalsy();
			expect(spyGet).toBeCalled();
		});

		it("data does not contain content", async () => {

			// [{"id":6,"content":"{\u0022data\u0022:[{\u0022filePath\u0022:\u0022/__starsky/00_demo/2.png\u0022,\u0022fileName\u0022:\u00222.png\u0022,\u0022fileHash\u0022:\u0022PFAKCYTKFSXOKEAR6X5N5OBZBA\u0022,\u0022fileCollectionName\u0022:\u00222\u0022,\u0022parentDirectory\u0022:\u0022/__starsky/00_demo\u0022,\u0022isDirectory\u0022:false,\u0022tags\u0022:\u00229\u0022,\u0022status\u0022:\u0022Ok\u0022,\u0022description\u0022:\u0022\u0022,\u0022title\u0022:\u0022\u0022,\u0022dateTime\u0022:\u00220001-01-01T00:00:00\u0022,\u0022addToDatabase\u0022:\u00222022-04-10T16:53:25.682824\u0022,\u0022lastEdited\u0022:\u00222022-04-11T17:55:36.379421Z\u0022,\u0022latitude\u0022:0,\u0022longitude\u0022:0,\u0022locationAltitude\u0022:0,\u0022locationCity\u0022:\u0022\u0022,\u0022locationState\u0022:\u0022\u0022,\u0022locationCountry\u0022:\u0022\u0022,\u0022colorClass\u0022:0,\u0022orientation\u0022:\u0022Horizontal\u0022,\u0022imageWidth\u0022:623,\u0022imageHeight\u0022:561,\u0022imageFormat\u0022:\u0022png\u0022,\u0022collectionPaths\u0022:[],\u0022sidecarExtensionsList\u0022:[],\u0022aperture\u0022:0,\u0022shutterSpeed\u0022:\u0022\u0022,\u0022isoSpeed\u0022:0,\u0022software\u0022:\u0022\u0022,\u0022makeModel\u0022:\u0022\u0022,\u0022make\u0022:\u0022\u0022,\u0022model\u0022:\u0022\u0022,\u0022lensModel\u0022:\u0022\u0022,\u0022focalLength\u0022:0,\u0022size\u0022:920017,\u0022imageStabilisation\u0022:0}],\u0022type\u0022:\u0022MetaUpdate\u0022}","dateTime":"2022-04-11T17:55:36.413662"}]
			
			const mockGetIConnectionDefault: Promise<IConnectionDefault> =
				Promise.resolve({
					statusCode: 200,
					data: {type: "Welcome", data: [{"content": "{}"}]}
				} as IConnectionDefault);
			const spyGet = jest
				.spyOn(FetchGet, "default")
				.mockImplementationOnce(() => mockGetIConnectionDefault);

			const result = await RestoreDataOnOpen(true, "any");

			expect(result).toBeFalsy();
			expect(spyGet).toBeCalled();
		});
	});
	
});
