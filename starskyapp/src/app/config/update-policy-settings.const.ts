import { isPackaged } from "../os-info/is-packaged";
import { UpdatePolicyIpcKey } from "./update-policy-ipc-key.const";

export const UpdatePolicySettings = `${UpdatePolicyIpcKey}:${isPackaged()}`;

const LastCheckedDate = `UPDATE_POLICY_LAST_CHECKED_DATE`;

export const LastCheckedDateSettings = `${LastCheckedDate}:${isPackaged()}`;
