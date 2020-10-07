import { Dispatch, MutableRefObject, SetStateAction } from 'react';
import { UrlQuery } from '../../shared/url-query';
import { useSocketsEventName } from './use-sockets.const';
import WebSocketService from './websocket-service';

function isKeepAliveMessage(item: any) {
  if (item.welcome || item.time) return true;
  return false;
}

function handleKeepAliveMessage(setKeepAliveTime: Dispatch<SetStateAction<Date>>, item: any) {
  if (!isKeepAliveMessage(item)) return;
  setKeepAliveTime(new Date());
}

const newWebSocketService = (): WebSocketService => {
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

export default function WsCurrentStart(socketConnected: boolean, setSocketConnected: Dispatch<SetStateAction<boolean>>,
  isEnabled: MutableRefObject<boolean>, setKeepAliveTime: Dispatch<SetStateAction<Date>>): WebSocketService {

  setSocketConnected(true);

  var socket = newWebSocketService();
  socket.onOpen(() => {
    console.log("[use-sockets] socket connection opened");
    if (!socketConnected) setSocketConnected(true);
  });

  socket.onClose((e) => {
    if (e.code === 1008 || e.code === 1009) {
      // 1008 = please login first
      // 1009 = feature toggle disabled
      console.log('[use-sockets] Disabled status: ' + e.code);
      isEnabled.current = false;
      return;
    }

    // console.log(e.code);
    if (socketConnected) setSocketConnected(false);
    console.log('[use-sockets] Web Socket Connection Closed ' + e.code);
  });

  socket.onError((_) => {
    if (socketConnected) setSocketConnected(false);
    console.log('[use-sockets] onError triggered');
  });

  socket.onMessage((e) => {
    var item = parseJson(e.data)

    if (isKeepAliveMessage(item)) {
      handleKeepAliveMessage(setKeepAliveTime, item);
      return;
    }
    parseMessage(item);
  })
  return socket;
}