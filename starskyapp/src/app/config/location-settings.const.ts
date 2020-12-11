import { isPackaged } from "../os-info/is-packaged";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "./location-ipc-keys.const";

export const LocationUrlSettingsKey = `${LocationUrlIpcKey}:${isPackaged()}`;

export const LocationIsRemoteSettingsKey = `${LocationIsRemoteIpcKey}:${isPackaged()}`;
