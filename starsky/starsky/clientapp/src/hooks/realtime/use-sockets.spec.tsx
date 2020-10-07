import { act } from '@testing-library/react';
import { ReactWrapper } from 'enzyme';
import * as DifferenceInDate from '../../shared/date';
import { mountReactHook } from '../___tests___/test-hook';
import useSockets, { IUseSockets } from './use-sockets';
import WebSocketService from './websocket-service';
import * as WsCurrentStart from './ws-current-start';

describe("useSockets", () => {

  let setupComponent: any;
  let hook: IUseSockets;
  let component: ReactWrapper;

  function mountComponent() {
    // next line to be removed in future release
    localStorage.setItem("use-sockets", "true");
    setupComponent = mountReactHook(useSockets, []); // Mount a Component with our hook
    hook = setupComponent.componentHook as IUseSockets;
    component = setupComponent.componentMount
  }

  it('default no error', () => {
    mountComponent();
    expect(hook.showSocketError).toBeFalsy();
    component.unmount();
  });

  it('ws current has been called', () => {
    var socketService = new WebSocketService("");
    var wsCurrent = jest.spyOn(WsCurrentStart, 'default').mockImplementationOnce(() => socketService);
    mountComponent();

    expect(wsCurrent).toBeCalled();
    expect(wsCurrent).toBeCalledTimes(1);
    expect(wsCurrent).toBeCalledWith(false, expect.any(Function), { "current": true }, expect.any(Function));

    wsCurrent.mockReset();
    component.unmount();
  });

  class FakeWebSocketService implements WebSocketService {
    public onOpen(callback: (ev: Event) => void): void {
      callback(new Event("t"))
    }
    public onClose(callback: (ev: CloseEvent) => void): void {
      callback(new CloseEvent("t"))
    }
    public onError(callback: (ev: Event) => void): void {
      callback(new CloseEvent("t"))
    }
    public close(): void {
    }

    public send(data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView): void {
    }
    public onMessage(callback: (event: MessageEvent<any>) => void): void {
    }
  }

  it('test retry when no response ', () => {
    console.log('test retry when no response');

    jest.useFakeTimers();
    var socketService = new FakeWebSocketService();

    // set the difference in time longer than 0.5 minutes
    jest.spyOn(DifferenceInDate, 'DifferenceInDate').mockImplementationOnce(() => 1)
    var wsCurrent = jest.spyOn(WsCurrentStart, 'default')
      .mockImplementationOnce(() => socketService)
      .mockImplementationOnce(() => socketService);

    mountComponent();

    act(() => {
      jest.advanceTimersByTime(60000);
    })

    expect(wsCurrent).toBeCalled();
    expect(wsCurrent).toBeCalledTimes(2);
    expect(wsCurrent).toBeCalledWith(false, expect.any(Function), { "current": true }, expect.any(Function));

    component.unmount();
    jest.useRealTimers();
  });

});