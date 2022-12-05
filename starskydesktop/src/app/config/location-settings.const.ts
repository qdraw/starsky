import { isPackaged } from "../os-info/is-packaged";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey,
} from "./location-ipc-keys.const";

/**
 * string
 */
export const LocationUrlSettingsKey = `${LocationUrlIpcKey}:${isPackaged().toString()}`;

/**
 * bool
 */
export const LocationIsRemoteSettingsKey = `${LocationIsRemoteIpcKey}:${isPackaged().toString()}`;
