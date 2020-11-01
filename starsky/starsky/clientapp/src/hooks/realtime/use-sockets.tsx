import { useEffect, useRef, useState } from 'react';
import { DifferenceInDate } from '../../shared/date';
import useInterval from '../use-interval';
import WebSocketService from './websocket-service';
import WsCurrentStart, { NewWebSocketService } from './ws-current-start';

export interface IUseSockets {
  showSocketError: boolean | null;
}

function Telemetry(message: string) {
  console.log(message);
  if (!(window as any).appInsights) return;
  var ai = (window as any).appInsights;
  ai.trackTrace({ message });
}


/**
 * Set an localStorage cookie when no websocket client is used
 */
function IsClientSideFeatureDisabled(): boolean {
  return localStorage.getItem("use-sockets") === "false"
}

/**
 * Use Socket as react hook
 */
const useSockets = (): IUseSockets => {

  const ws = useRef({} as WebSocketService);
  // When the connection is lost
  const [socketConnected, setSocketConnected] = useState(false);
  // show a error message
  const [showSocketError, setShowSocketError] = useState(false);
  // server side feature toggle to disable/enable client
  const isEnabled = useRef(true);
  // time the server has pinged me back
  const [keepAliveTime, setKeepAliveTime] = useState(new Date());

  // useState does not update in a sync way
  const countRetry = useRef(0);

  useInterval(doIntervalCheck, 30000);

  function doIntervalCheck() {
    if (!isEnabled.current || !ws.current || !ws.current.close) return;
    setShowSocketError(countRetry.current >= 1)

    if (DifferenceInDate(keepAliveTime.getTime()) > 0.5) {
      Telemetry('[use-sockets] --retry sockets');
      setSocketConnected(false);
      countRetry.current++;
      ws.current.close();
      ws.current = WsCurrentStart(socketConnected, setSocketConnected, isEnabled, setKeepAliveTime, NewWebSocketService);
    }
    else {
      countRetry.current = 0;
    }
  }

  useEffect(() => {
    console.log(`[use-sockets] is disabled => ${IsClientSideFeatureDisabled()}`);
    // check to be removed in future version
    if (IsClientSideFeatureDisabled()) return;

    ws.current = WsCurrentStart(socketConnected, setSocketConnected, isEnabled, setKeepAliveTime, NewWebSocketService);
    return () => {
      console.log('[use-sockets] --end');
      ws.current.close();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return {
    showSocketError
  };
};

export default useSockets;