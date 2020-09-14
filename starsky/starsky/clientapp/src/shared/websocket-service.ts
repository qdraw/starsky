
export class WebSocketService {

  private websocket?: WebSocket;
  constructor(url: string, protocols?: string | string[]) {
    try {
      this.websocket = new WebSocket(url, protocols);
    } catch (error) {
    }
  }

  public onOpen(callback: (ev: Event) => void) {
    if (!this.websocket) return;
    return this.websocket.onopen = callback;
  }

  public onClose(callback: (ev: CloseEvent) => void) {
    if (!this.websocket) return;
    return this.websocket.onclose = callback;
  }

  public onError(callback: (ev: Event) => void) {
    if (!this.websocket) {
      return callback(new Event('err'))
    }
    return this.websocket.onerror = callback;
  }


  public close(callback: (code?: number, reason?: string) => void) {
    if (!this.websocket) return;
    return this.websocket.close = callback;
  }


  public onMessage(callback: (event: MessageEvent) => void) {
    if (!this.websocket) {
      return;
    }
    return this.websocket.onmessage = callback;
  }

}