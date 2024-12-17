import { Dispatch, SetStateAction, useEffect, useRef, useState } from "react";
import { DifferenceInDate } from "../../shared/date";
import useInterval from "../use-interval";
import { IsClientSideFeatureDisabled } from "./internal/is-client-side-feature-disabled.ts";
import WebSocketService from "./websocket-service";
import WsCurrentStart, { NewWebSocketService } from "./ws-current-start";

export interface IUseSockets {
  showSocketError: boolean | null;
  setShowSocketError: Dispatch<SetStateAction<boolean | null>>;
}

/**
 * Use Socket as react hook
 */
const useSockets = (): IUseSockets => {
  const ws = useRef({} as WebSocketService);
  // When the connection is lost
  const [socketConnected, setSocketConnected] = useState(false);
  // show a error message
  // (don't update this field every render to avoid endless re-rendering)
  const [showSocketError, setShowSocketError] = useState<boolean | null>(false);
  // server side feature toggle to disable/enable client
  const isEnabled = useRef(true);
  // time the server has pinged me back (it should every 20 seconds)
  const [keepAliveTime, setKeepAliveTime] = useState(new Date());

  const [keepAliveServerTime, setKeepAliveServerTime] = useState("");

  // number of failures
  const [countRetry, setCountRetry] = useState(0);

  const startDiffTime = 30000;
  const [diffTimeInMs, setDiffTimeInMs] = useState(startDiffTime);

  useInterval(doIntervalCheck, startDiffTime);

  function doIntervalCheck() {
    if (!isEnabled.current || !ws.current?.close) {
      return;
    }

    // display notification
    setShowSocketError((prevCount) => {
      // set it on null to hide the error message it until connection is picked up again
      if (prevCount == null) {
        return null;
      }
      return countRetry >= 2;
    });

    if (DifferenceInDate(keepAliveTime.getTime()) > diffTimeInMs / 60000) {
      console.log(`[use-sockets] --retry sockets ${diffTimeInMs / 60000}`);

      setSocketConnected(false);
      setCountRetry((prev) => prev + 1);

      ws.current.close();
      ws.current = WsCurrentStart(
        socketConnected,
        setSocketConnected,
        isEnabled,
        setKeepAliveTime,
        NewWebSocketService,
        keepAliveServerTime,
        setKeepAliveServerTime
      );
    } else {
      setCountRetry(0);
      // to reset error message when its null
      setShowSocketError((prevCount) => {
        if (prevCount == null) {
          return false;
        }
        return prevCount;
      });
      setDiffTimeInMs(startDiffTime);
    }
  }

  useEffect(() => {
    console.log(`[use-sockets] is disabled => ${IsClientSideFeatureDisabled()}`);
    // option to disable in client side
    if (IsClientSideFeatureDisabled()) return;

    ws.current = WsCurrentStart(
      socketConnected,
      setSocketConnected,
      isEnabled,
      setKeepAliveTime,
      NewWebSocketService,
      keepAliveServerTime,
      setKeepAliveServerTime
    );

    // when effect ends ->
    return () => {
      console.log("[use-sockets] --end");
      ws.current.close();
    };

    // When switching the feature toggle
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [localStorage.getItem("use-sockets")]);

  return {
    showSocketError,
    setShowSocketError
  };
};

export default useSockets;
