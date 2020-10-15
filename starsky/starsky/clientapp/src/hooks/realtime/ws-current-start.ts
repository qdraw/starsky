import { Dispatch, MutableRefObject, SetStateAction } from 'react';
import { UrlQuery } from '../../shared/url-query';
import { useSocketsEventName } from './use-sockets.const';
import WebSocketService from './websocket-service';

function isKeepAliveMessage(item: any) {
  if (!item) return false;
  if (item.welcome || item.time) return true;
  return false;
}

export function HandleKeepAliveMessage(setKeepAliveTime: Dispatch<SetStateAction<Date>>, item: any) {
  if (!isKeepAliveMessage(item)) return;
  setKeepAliveTime(new Date());
}

export const NewWebSocketService = (): WebSocketService => {
  return new WebSocketService(new UrlQuery().UrlRealtime());
}

function parseJson(data: string): any {
  try {
    return JSON.parse(data);
  } catch (error) {
    console.log(error);
  }
}

function parseMessage(item: string) {
  if (!item) return;
  console.log('update', item);
  document.body.dispatchEvent(new CustomEvent(useSocketsEventName, { detail: item, bubbles: false }))
}

export function FireOnClose(e: CloseEvent, socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>, isEnabled: MutableRefObject<boolean>,) {
  if (e.code === 1008 || e.code === 1009) {
    // 1008 = please login first
    // 1009 = feature toggle disabled
    console.log('[use-sockets] Disabled status: ' + e.code);
    isEnabled.current = false;
    return;
  }
  if (socketConnected) setSocketConnected(false);
  console.log('[use-sockets] Web Socket Connection Closed ' + e.code);
}

export function FireOnError(socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>) {
  if (socketConnected) setSocketConnected(false);
  console.log('[use-sockets] onError triggered');
}

export function FireOnOpen(socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>) {
  console.log("[use-sockets] socket connection opened");
  if (!socketConnected) setSocketConnected(true);
}

export function FireOnMessage(e: Event, setKeepAliveTime: Dispatch<SetStateAction<Date>>) {
  var item = parseJson((e as any).data)

  if (isKeepAliveMessage(item)) {
    HandleKeepAliveMessage(setKeepAliveTime, item);
    return;
  }
  parseMessage(item);
}

export default function WsCurrentStart(socketConnected: boolean, setSocketConnected: Dispatch<SetStateAction<boolean>>,
  isEnabled: MutableRefObject<boolean>, setKeepAliveTime: Dispatch<SetStateAction<Date>>,
  InsertNewWebSocketService: () => WebSocketService): WebSocketService {

  setSocketConnected(true);

  var socket = InsertNewWebSocketService();
  socket.onOpen(() => FireOnOpen(socketConnected, setSocketConnected));
  socket.onClose((e) => FireOnClose(e, socketConnected, setSocketConnected, isEnabled));
  socket.onError(() => FireOnError(socketConnected, setSocketConnected));
  socket.onMessage((e) => FireOnMessage(e, setKeepAliveTime));

  return socket;
}