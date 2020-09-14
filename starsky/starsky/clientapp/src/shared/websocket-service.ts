
export class WebSocketService {

  private websocket: WebSocket;
  constructor(url: string, protocols?: string | string[]) {
    this.websocket = new WebSocket(url, protocols);
  }

  public onOpen(callback: (ev: Event) => void) {
    return this.websocket.onopen = callback;
  }

  public onClose(callback: (ev: CloseEvent) => void) {
    return this.websocket.onclose = callback;
  }

  public onError(callback: (ev: Event) => void) {
    return this.websocket.onerror = callback;
  }


  public close(callback: (code?: number, reason?: string) => void) {
    return this.websocket.close = callback;
  }


  public onMessage(callback: (event: MessageEvent) => void) {
    if (!this.websocket) {
      return;
    }
    return this.websocket.onmessage = callback;
  }

}