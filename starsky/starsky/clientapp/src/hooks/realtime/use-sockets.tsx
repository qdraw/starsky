import { Dispatch, SetStateAction, useEffect, useRef, useState } from "react";
import { DifferenceInDate } from "../../shared/date";
import useInterval from "../use-interval";
import WebSocketService from "./websocket-service";
import WsCurrentStart, { NewWebSocketService } from "./ws-current-start";

export interface IUseSockets {
  showSocketError: boolean | null;
  setShowSocketError: Dispatch<SetStateAction<boolean>>;
}

/**
 * Set an localStorage cookie when no websocket client is used
 */
function IsClientSideFeatureDisabled(): boolean {
  return localStorage.getItem("use-sockets") === "false";
}

/**
 * Use Socket as react hook
 */
const useSockets = (): IUseSockets => {
  let ws = useRef({} as WebSocketService);
  // When the connection is lost
  const [socketConnected, setSocketConnected] = useState(false);
  // show a error message
  // (dont update this field every render to avoid endless re-rendering)
  const [showSocketError, setShowSocketError] = useState(false);
  // server side feature toggle to disable/enable client
  const isEnabled = useRef(true);
  // time the server has pinged me back (it should every 20 seconds)
  const [keepAliveTime, setKeepAliveTime] = useState(new Date());

  // number of failures
  const [countRetry, setCountRetry] = useState(0);

  const startDiffTime = 20000;
  const [diffTimeInMs, setDiffTimeInMs] = useState(startDiffTime);

  useInterval(doIntervalCheck, startDiffTime);

  function doIntervalCheck() {
    console.log(isEnabled, ws, countRetry);
    if (!isEnabled.current || !ws.current || !ws.current.close) {
      return;
    }

    // display notification
    setShowSocketError(countRetry >= 1);

    if (DifferenceInDate(keepAliveTime.getTime()) > diffTimeInMs / 60000) {
      console.log(`[use-sockets] --retry sockets ${diffTimeInMs / 60000}`);

      setSocketConnected(false);

      setDiffTimeInMs((prev) => prev + 500);
      setCountRetry((prev) => prev + 1);

      ws.current.close();
      ws.current = WsCurrentStart(
        socketConnected,
        setSocketConnected,
        isEnabled,
        setKeepAliveTime,
        NewWebSocketService
      );
    } else {
      setCountRetry(0);
      setDiffTimeInMs(startDiffTime);
    }
  }

  useEffect(() => {
    console.log(
      `[use-sockets] is disabled => ${IsClientSideFeatureDisabled()}`
    );
    // option to disable in client side
    if (IsClientSideFeatureDisabled()) return;

    ws.current = WsCurrentStart(
      socketConnected,
      setSocketConnected,
      isEnabled,
      setKeepAliveTime,
      NewWebSocketService
    );
    return () => {
      console.log("[use-sockets] --end");
      ws.current.close();
    };

    // When switching the feature toggle
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [localStorage.getItem("use-sockets")]);

  return {
    showSocketError,
    setShowSocketError
  };
};

export default useSockets;
