import { isPackaged } from "../os-info/is-packaged";

const RememberUrl = `rememberUrl:${isPackaged()}`
export default RememberUrl;