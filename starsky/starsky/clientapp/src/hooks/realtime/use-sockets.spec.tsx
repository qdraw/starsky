import { mountReactHook } from '../___tests___/test-hook';
import useSockets, { IUseSockets } from './use-sockets';
import WebSocketService from './websocket-service';
import * as WsCurrentStart from './ws-current-start';

describe("useSockets", () => {

  let setupComponent;
  let hook: IUseSockets;

  beforeEach(() => {
    // to be removed in future release
    localStorage.setItem("use-sockets", "true");

    setupComponent = mountReactHook(useSockets, []); // Mount a Component with our hook
    hook = setupComponent.componentHook as IUseSockets;
  });

  it('default no error', () => {
    expect(hook.showSocketError).toBeFalsy();
  });

  it('default no error 2', () => {
    var wsCurrent = jest.spyOn(WsCurrentStart, 'default').mockImplementationOnce(() => new WebSocketService(""));



    expect(hook.showSocketError).toBeFalsy();

    expect(wsCurrent).toBeCalled();
  });

});