
export class WebSocketService {

  private websocket?: WebSocket;
  constructor(url: string, protocols?: string | string[]) {
    try {
      this.websocket = new WebSocket(url, protocols);
    } catch (error) {
    }
  }


  public onOpen(callback: (ev: Event) => void): void {
    if (!this.websocket) return;
    this.websocket.onopen = callback;
  }

  public onClose(callback: (ev: CloseEvent) => void): void {
    if (!this.websocket) return;
    this.websocket.onclose = callback;
  }

  public onError(callback: (ev: Event) => void): void {
    if (!this.websocket) {
      return callback(new Event('err'))
    }
    this.websocket.onerror = callback;
  }


  public close(): void {
    if (!this.websocket) {
      return;
    };
    this.websocket.close();
  }

  public send(data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView): void {
    if (!this.websocket) {
      return;
    };
    this.websocket.send(data);
  }

  public onMessage(callback: (event: MessageEvent) => void): void {
    if (!this.websocket) {
      return;
    }
    this.websocket.onmessage = callback;
  }

}