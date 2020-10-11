import WsCurrentStart, * as NewWebSocketService from './ws-current-start';
import { FakeWebSocketService } from './___tests___/fake-web-socket-service';

describe("WsCurrentStart", () => {
  describe("default", () => {

    it("onMessage", () => {
      jest.spyOn(NewWebSocketService, 'NewWebSocketService').mockImplementationOnce(() => new FakeWebSocketService());

      var wsCurrentStart = WsCurrentStart(true, jest.fn(), { current: true }, jest.fn());
      wsCurrentStart
    });
  });
});