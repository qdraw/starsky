import { isPackaged } from "../os-info/is-packaged";
import {
  LocationIsRemoteIpcKey,
  LocationLocalHostIpcKey,
  LocationUrlIpcKey,
} from "./location-ipc-keys.const";

/**
 * string
 */
export const LocationUrlSettingsKey = `${LocationUrlIpcKey}:${isPackaged().toString()}`;

/**
 * string
 */
export const LocationLocalHostSettingsKey = `${LocationLocalHostIpcKey}:${isPackaged().toString()}`;


/**
 * bool
 */
export const LocationIsRemoteSettingsKey = `${LocationIsRemoteIpcKey}:${isPackaged().toString()}`;
