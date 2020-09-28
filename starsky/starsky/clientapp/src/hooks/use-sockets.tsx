import { useEffect, useRef, useState } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { DifferenceInDate } from '../shared/date';
import { WebSocketService } from '../shared/realtime/websocket-service';
import { UrlQuery } from '../shared/url-query';
import useInterval from './use-interval';

export const useSocketsEventName = 'USE_SOCKETS';
export const useSocketsMaxTry = 10;

export interface IUseSockets {
  showSocketError: boolean | null;
}

const newWebSocketService = (): WebSocketService => {
  return new WebSocketService(new UrlQuery().UrlRealtime());
}

function parseMessage(data: string) {
  var fileIndexItem: IFileIndexItem | undefined;

  try {
    fileIndexItem = JSON.parse(data).data;
  } catch (error) {
    console.log(error);
  }
  if (!fileIndexItem) return;
  console.log('update', fileIndexItem.filePath);
  document.body.dispatchEvent(new CustomEvent(useSocketsEventName, { detail: fileIndexItem, bubbles: false }))
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

  const [isEnabled, setIsEnabled] = useState(true);

  const [pong, setPong] = useState(new Date());

  // useState does not update in a sync way
  const countRetry = useRef(0);
  // const [countRetry, setCountRetry] = useState(0);


  function intervalCheck() {
    if (!ws.current) return;
    if (!isEnabled) return;
    setShowSocketError(countRetry.current >= 1)

    if (DifferenceInDate(pong.getTime()) > 0.3) {
      console.log('[use-sockets] --retry sockets');
      setSocketConnected(false);
      countRetry.current++;
      ws.current.close();
      ws.current = wsCurrentStart();
    }

  };

  // ALSO UPDATE DIFF time
  useInterval(intervalCheck, 15000);
  // 15000 milliseconds = 0.25 minutes

  function isPong(data: string) {
    if (!data.startsWith("\"pong")) return false;
    return true;
  }

  function handlePong(data: string) {
    if (!isPong(data)) return;
    setPong(new Date());
    countRetry.current = 0;
  }

  function wsCurrentStart(): WebSocketService {
    var socket = newWebSocketService();
    socket.onOpen(() => {
      console.log("[use-sockets] socket connection opened send ping");

      if (!socketConnected) setSocketConnected(true);
    });

    socket.onClose((e) => {
      if (e.code === 1008 || e.code === 1006) {
        // 1008 = please login first
        // 1006 = feature toggle disabled
        console.log('------ login or feature toggle');
        setIsEnabled(false);
        return;
      }


      console.log(e.code);

      setSocketConnected(false);
      console.log('[use-sockets] Web Socket Connection Closed');
    });

    socket.onError((e) => {
      setSocketConnected(false);
      console.log('[use-sockets] onError triggerd');
    });

    socket.onMessage((e) => {
      if (isPong(e.data)) {
        handlePong(e.data);
        return;
      }
      parseMessage(e.data);
    })
    return socket;
  }

  useEffect(() => {
    console.log('[use-sockets] run effect');

    // FetchGet(new UrlQuery().UrlRealtimeStatus()).then((data) => {
    //   if (data.statusCode === 401 || data.statusCode === 403) {
    //     setIsEnabled(false);
    //   }
    // });

    var socket1 = wsCurrentStart();
    ws.current = socket1;
    return () => {
      console.log('[use-sockets] --end');
      socket1.close();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return {
    showSocketError
  };
};

export default useSockets;