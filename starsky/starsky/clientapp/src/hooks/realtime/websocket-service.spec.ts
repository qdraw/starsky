import WebSocketService from "./websocket-service";

describe("WebSocketService", () => {
	describe("undefined context", () => {
		var webSocketService = new WebSocketService("");

		it("onOpen", () => {
			var callback = jest.fn();
			webSocketService.onOpen(callback);
			expect(callback).toBeCalledTimes(0);
		});

		it("onClose", () => {
			var callback = jest.fn();
			webSocketService.onClose(callback);
			expect(callback).toBeCalledTimes(0);
		});

		it("onError", () => {
			var callback = jest.fn();
			webSocketService.onError(callback);
			expect(callback).toBeCalled();
		});

		it("send", () => {
			webSocketService.send("");
			// should not crash
		});

		it("onMessage", () => {
			var callback = jest.fn();
			webSocketService.onMessage(callback);
			expect(callback).toBeCalledTimes(0);
		});
	});
});
