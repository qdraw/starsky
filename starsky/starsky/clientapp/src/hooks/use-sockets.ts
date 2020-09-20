import { useEffect, useRef, useState } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { DifferenceInDate } from '../shared/date';
import FetchGet from '../shared/fetch-get';
import { UrlQuery } from '../shared/url-query';
import { WebSocketService } from '../shared/websocket-service';
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

  // const timeoutRef = useRef({} as NodeJS.Timeout);

  const [pong, setPong] = useState(new Date());

  // useState does not update in a sync way
  const countRetry = useRef(0);
  // const [countRetry, setCountRetry] = useState(0);


  useInterval(() => {
    if (!ws.current) return;

    if (socketConnected) {
      ws.current.send("ping");
      return;
    }

    console.log(pong.getTime());
    console.log(DifferenceInDate(pong.getTime()));


    if (DifferenceInDate(pong.getTime()) > 0.2) {
      setSocketConnected(false);
      countRetry.current++;
      ws.current.close();

      setShowSocketError(countRetry.current > 2)

      ws.current = wsCurrentStart();
    }

    // 10000 milliseconds = 0.16 minutes
  }, 10000)



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
    socket.onOpen((e) => {
      console.log(e);
      socket.send("ping");
      if (!socketConnected) setSocketConnected(true);
    });

    socket.onClose(() => {
      setSocketConnected(false);
      console.log('Web Socket Connection Closed');
    });

    socket.onError((e) => {
      setSocketConnected(false);
      console.log(e);
    });

    socket.onMessage((e) => {
      console.log(e.data);
      if (isPong(e.data)) {
        handlePong(e.data);
        return;
      }
      parseMessage(e.data);
    })
    return socket;
  }

  useEffect(() => {
    console.log('--run effect');

    FetchGet(new UrlQuery().UrlRealtimeStatus()).then((data) => {
      console.log(data.statusCode);

    });

    var socket1 = wsCurrentStart();
    ws.current = socket1;
    return () => {
      socket1.close();
    };
  }, []);
  /// /    // eslint-disable-next-line react-hooks/exhaustive-deps

  return {
    showSocketError: false
  };
};

export default useSockets;