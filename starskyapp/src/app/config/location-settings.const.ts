import { isPackaged } from "../os-info/is-packaged";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "./location-ipc-keys.const";

/**
 * string
 */
export const LocationUrlSettingsKey = `${LocationUrlIpcKey}:${isPackaged().valueOf.toString()}`;

/**
 * bool
 */
export const LocationIsRemoteSettingsKey = `${LocationIsRemoteIpcKey}:${isPackaged().valueOf.toString()}`;
