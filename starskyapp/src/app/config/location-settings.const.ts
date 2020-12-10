import { isPackaged } from "../os-info/is-packaged";

export const LocationUrl = `LOCATION_URL:${isPackaged()}`;

export const LocationIsRemote = `LOCATION_IS_REMOTE:${isPackaged()}`;
