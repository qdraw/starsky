import { useEffect, useRef, useState } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchGet from '../shared/fetch-get';
import { UrlQuery } from '../shared/url-query';
import { WebSocketService } from '../shared/websocket-service';

export const useSocketsEventName = 'USE_SOCKETS';
export const useSocketsMaxTry = 10;

export interface IUseSockets {
  socketsFailed: boolean;
}

/**
 * 
 * @param effectStates where to watch for
 * @param socketService a socket Service to replace
 */
const useSockets = (effectStates: boolean[] | string[], socketService?: WebSocketService): IUseSockets => {

  const ws = useRef({} as WebSocketService);

  const newWebSocketService = (): WebSocketService => {
    return new WebSocketService(new UrlQuery().UrlRealtime());
  }

  const [socketsFailed, setSocketsFailed] = useState(false);
  const intervalRef = useRef({} as NodeJS.Timeout);

  function onOpen() {
    ws.current.onOpen(() => {
      console.log('open websocket connection');
      clearInterval(intervalRef.current);
      countRetry.current = 0;
    })
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

  // useState does not update in a sync way
  const countRetry = useRef(0);

  useEffect(() => {

    FetchGet(new UrlQuery().UrlRealtimeStatus()).then((data) => {
      console.log(data.statusCode);

    });

    ws.current = socketService ? socketService : newWebSocketService();
    ws.current.onClose(() => {
      intervalRef.current = setInterval(() => {
        if (countRetry.current > useSocketsMaxTry) {
          clearInterval(intervalRef.current);
          setSocketsFailed(true);
          return;
        }
        console.log('retry number: ', countRetry.current);
        ws.current = newWebSocketService();
        onOpen();
        ws.current.onMessage((event) => parseMessage(event.data));
        countRetry.current++;
        // change to 10000
      }, 10000)
    })

    onOpen();
    ws.current.onMessage((event) => parseMessage(event.data));

    return () => {
      ws.current.close(() => { });
    };

  }, [...effectStates, socketService]);

  return {
    socketsFailed
  };
};

export default useSockets;