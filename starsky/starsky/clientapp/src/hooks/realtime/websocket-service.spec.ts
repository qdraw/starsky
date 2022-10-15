import WebSocketService from "./websocket-service";

describe("WebSocketService", () => {
  describe("undefined context", () => {
    const webSocketService = new WebSocketService("");

    it("onOpen", () => {
      const callback = jest.fn();
      webSocketService.onOpen(callback);
      expect(callback).toBeCalledTimes(0);
    });

    it("onClose", () => {
      const callback = jest.fn();
      webSocketService.onClose(callback);
      expect(callback).toBeCalledTimes(0);
    });

    it("onError", () => {
      const callback = jest.fn();
      webSocketService.onError(callback);
      expect(callback).toBeCalled();
    });

    it("send", () => {
      webSocketService.send("");
      // should not crash
    });

    it("onMessage", () => {
      const callback = jest.fn();
      webSocketService.onMessage(callback);
      expect(callback).toBeCalledTimes(0);
    });
  });
});
