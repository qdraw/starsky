import WebSocketService from '../websocket-service'

export class FakeWebSocketService implements WebSocketService {
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