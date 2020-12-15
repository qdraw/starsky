import { isPackaged } from "../os-info/is-packaged";

const RememberUrl = `REMEMBER_URL:${isPackaged()}`;
export default RememberUrl;
