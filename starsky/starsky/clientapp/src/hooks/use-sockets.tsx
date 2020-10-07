import { useEffect, useRef, useState } from 'react';
import { DifferenceInDate } from '../shared/date';
import { WebSocketService } from '../shared/realtime/websocket-service';
import { UrlQuery } from '../shared/url-query';

export const useSocketsEventName = 'USE_SOCKETS';
export const useSocketsMaxTry = 10;

export interface IUseSockets {
  showSocketError: boolean | null;
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
  // document.body.dispatchEvent(new CustomEvent(useSocketsEventName, { detail: fileIndexItem, bubbles: false }))
}


/**
 * 
 * @param effectStates where to watch for
 * @param inputSocketService a socket Service to replace
 */
const useSockets = (): IUseSockets => {

  const ws = useRef({} as WebSocketService);
  const [socketConnected, setSocketConnected] = useState(false);
  const [showSocketError, setShowSocketError] = useState(false);

  const isEnabled = useRef(true);

  const [keepAliveTime, setKeepAliveTime] = useState(new Date());

  // useState does not update in a sync way
  const countRetry = useRef(0);

  function doIntervalCheck() {
    console.log(isEnabled.current);
    if (!ws.current) return;
    if (!isEnabled.current) return;
    setShowSocketError(countRetry.current >= 1)

    if (DifferenceInDate(keepAliveTime.getTime()) > 0.55) {
      Telemetry('[use-sockets] --retry sockets');
      setSocketConnected(false);
      countRetry.current++;
      ws.current.close();
      ws.current = wsCurrentStart();
    }
  };

  function Telemetry(message: string) {
    console.log(message);
    if (!(window as any).appInsights) return;
    var ai = (window as any).appInsights;
    ai.trackTrace({ message });
  }

  function isKeepAliveMessage(item: any) {
    if (item.welcome || item.time) return true;
    return false;
  }

  function handleKeepAliveMessage(item: any) {
    if (!isKeepAliveMessage(item)) return;
    setKeepAliveTime(new Date());
  }

  function wsCurrentStart(): WebSocketService {

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
        handleKeepAliveMessage(item);
        return;
      }
      parseMessage(item);
    })
    return socket;
  }

  function isFeatureEnabled(): boolean {
    return localStorage.getItem("use-sockets") !== null
  }

  useEffect(() => {
    console.log(`[use-sockets] run effect - ${isFeatureEnabled()}`);
    if (!isFeatureEnabled()) return;

    ws.current = wsCurrentStart();
    var intervalCheck = setInterval(doIntervalCheck, 30000)

    return () => {
      console.log('[use-sockets] --end');
      ws.current.close();
      clearInterval(intervalCheck);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return {
    showSocketError
  };
};

export default useSockets;