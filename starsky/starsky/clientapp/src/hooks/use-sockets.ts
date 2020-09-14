import { useEffect, useRef } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { WebSocketService } from '../shared/websocket-service';

export const useSocketsEventName = 'USE_SOCKETS';

export interface IUseSockets {
}

const useSockets = (socketService?: WebSocketService): IUseSockets | null => {

  const ws = useRef({} as WebSocketService);

  const newWebSocketService = (): WebSocketService => {
    return new WebSocketService("ws://localhost:5000/realtime");
  }

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
    console.log(data);

    try {
      fileIndexItem = JSON.parse(data).data;
    } catch (error) {
      console.log(error);

    }
    if (!fileIndexItem) return;
    console.log(fileIndexItem);
    document.body.dispatchEvent(new CustomEvent(useSocketsEventName, { detail: fileIndexItem, bubbles: false }))
  }



  // useState does not update in a sync way
  const countRetry = useRef(0);

  useEffect(() => {
    ws.current = socketService ? socketService : newWebSocketService();
    ws.current.onClose(() => {

      intervalRef.current = setInterval(() => {
        if (countRetry.current > 10) {
          clearInterval(intervalRef.current);
          throw new Error('connection issues tried more than 10 times');
        }
        console.log('retry number: ', countRetry.current);
        ws.current = newWebSocketService();
        onOpen();
        ws.current.onMessage((event) => parseMessage(event.data));
        countRetry.current++;
      }, 10000)
    })

    onOpen();
    ws.current.onMessage((event) => parseMessage(event.data));

    return () => {
      console.log('--close');

      ws.current.close(() => { });
    };

  }, [socketService]);

  return {};
};

export default useSockets;