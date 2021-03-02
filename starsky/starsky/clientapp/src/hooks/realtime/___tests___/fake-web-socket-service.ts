import WebSocketService from "../websocket-service";

export class FakeWebSocketService implements WebSocketService {
  private onOpenEvent: Event;

  private onCloseEvent: CloseEvent;

  public OnOpenCalled: boolean = false;
  public OnCloseCalled: boolean = false;
  public OnErrorCalled: boolean = false;

  constructor(
    onOpenEvent: Event = new Event("t"),
    onCloseEvent: CloseEvent = new CloseEvent("t")
  ) {
    this.onOpenEvent = onOpenEvent;
    this.onCloseEvent = onCloseEvent;
  }

  public onOpen(callback: (ev: Event) => void): void {
    this.OnOpenCalled = true;
    callback(this.onOpenEvent);
  }
  public onClose(callback: (ev: CloseEvent) => void): void {
    this.OnCloseCalled = true;
    callback(this.onCloseEvent);
  }
  public onError(callback: (ev: Event) => void): void {
    this.OnErrorCalled = true;
    callback(this.onCloseEvent);
  }

  public close(): void {
    // it should do nothing
  }

  public send(
    data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView
  ): void {
    // it should do nothing
  }

  public onMessage(callback: (event: MessageEvent) => void): void {
    // it should do nothing
  }
}
