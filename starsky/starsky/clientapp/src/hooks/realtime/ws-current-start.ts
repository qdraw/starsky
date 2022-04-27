import { Dispatch, MutableRefObject, SetStateAction } from "react";
import { IApiNotificationResponseModel } from "../../interfaces/IApiNotificationResponseModel";
import { IFileIndexItem } from "../../interfaces/IFileIndexItem";
import FetchGet from "../../shared/fetch-get";
import { UrlQuery } from "../../shared/url-query";
import { useSocketsEventName } from "./use-sockets.const";
import WebSocketService from "./websocket-service";

export function isKeepAliveMessage(item: any) {
  if (!item || !item.type) return false;
  return item.type === "Welcome" || item.type === "Heartbeat";
}

export function HandleKeepAliveMessage(
  setKeepAliveTime: Dispatch<SetStateAction<Date>>,
  item: any
) {
  if (!isKeepAliveMessage(item)) return;
  setKeepAliveTime(new Date());
}

export function HandleKeepAliveServerMessage(
  setKeepAliveServerTime: Dispatch<SetStateAction<string>>,
  item: any
) {
  if (!isKeepAliveMessage(item) || !item.data || !item.data.dateTime) return;
  setKeepAliveServerTime(item.data.dateTime);
}

export const NewWebSocketService = (): WebSocketService => {
  return new WebSocketService(new UrlQuery().UrlRealtime());
};

/**
 *
 * @param data
 * @returns
 */
export function parseJson(data: string): any {
  try {
    return JSON.parse(data);
  } catch (error) {
    console.log(error);
    return null;
  }
}

function PushMessage(item: IApiNotificationResponseModel<IFileIndexItem[]>) {
  if (!item) return;
  console.log(`[use-sockets] update ${item.type}`, item.data);
  document.body.dispatchEvent(
    new CustomEvent(useSocketsEventName, { detail: item, bubbles: false })
  );
}

export function FireOnClose(
  e: CloseEvent,
  socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>,
  isEnabled: MutableRefObject<boolean>
) {
  if (e.code === 1008 || e.code === 1009) {
    // 1008 = please login first
    // 1009 = feature toggle disabled
    console.log("[use-sockets] Disabled status: " + e.code);
    isEnabled.current = false;
    return;
  }
  if (socketConnected) setSocketConnected(false);
  console.log("[use-sockets] Web Socket Connection Closed " + e.code);
}

export function FireOnError(
  socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>
) {
  if (socketConnected) setSocketConnected(false);
  console.log("[use-sockets] onError triggered");
}

export function FireOnOpen(
  socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>
) {
  console.log("[use-sockets] socket connection opened");
  if (!socketConnected) {
    setSocketConnected(true);
  }
}

export async function RestoreDataOnOpen(
  socketConnected: boolean,
  keepAliveServerTime: string
): Promise<boolean> {
  if (!socketConnected || !keepAliveServerTime) {
    return false;
  }
  const result = await FetchGet(
    new UrlQuery().UrlNotificationsGetApi(keepAliveServerTime)
  );

  if (
    result.statusCode !== 200 ||
    !result.data ||
    !Array.isArray(result.data)
  ) {
    return false;
  }

  let anyResults = false;
  for (const dataItem of result.data) {
    if (!dataItem.content) {
      console.log(dataItem);
      console.log("no content");
      continue;
    }
    const item = parseJson(dataItem.content);
    PushMessage(item);
    anyResults = true;
  }
  console.log(result.data);

  return anyResults;
}

export function FireOnMessage(
  e: Event,
  setKeepAliveTime: Dispatch<SetStateAction<Date>>,
  setKeepAliveServerTime: Dispatch<SetStateAction<string>>
) {
  const item = parseJson((e as any).data);

  if (isKeepAliveMessage(item)) {
    HandleKeepAliveMessage(setKeepAliveTime, item);
    HandleKeepAliveServerMessage(setKeepAliveServerTime, item);
    return;
  }
  PushMessage(item);
}

export default function WsCurrentStart(
  socketConnected: boolean,
  setSocketConnected: Dispatch<SetStateAction<boolean>>,
  isEnabled: MutableRefObject<boolean>,
  setKeepAliveTime: Dispatch<SetStateAction<Date>>,
  InsertNewWebSocketService: () => WebSocketService,
  keepAliveServerTime: string,
  setKeepAliveServerTime: Dispatch<SetStateAction<string>>
): WebSocketService {
  setSocketConnected(true);

  const socket = InsertNewWebSocketService();
  socket.onOpen(async () => {
    FireOnOpen(socketConnected, setSocketConnected);
    await RestoreDataOnOpen(socketConnected, keepAliveServerTime);
  });
  socket.onClose((e) =>
    FireOnClose(e, socketConnected, setSocketConnected, isEnabled)
  );
  socket.onError(() => FireOnError(socketConnected, setSocketConnected));
  socket.onMessage((e) =>
    FireOnMessage(e, setKeepAliveTime, setKeepAliveServerTime)
  );

  return socket;
}
