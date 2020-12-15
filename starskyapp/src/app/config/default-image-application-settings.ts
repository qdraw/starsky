import { isPackaged } from "../os-info/is-packaged";
import { DefaultImageApplicationIpcKey } from "./default-image-application-settings-ipc-key.const";

const DefaultImageApplicationSetting = `${DefaultImageApplicationIpcKey}:${isPackaged()}`;
export default DefaultImageApplicationSetting;
