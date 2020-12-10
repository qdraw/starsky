import { isPackaged } from "../os-info/is-packaged";

export const LocationUrlSettingsKey = `LOCATION_URL:${isPackaged()}`;

export const LocationIsRemoteSettingsKey = `LOCATION_IS_REMOTE:${isPackaged()}`;

