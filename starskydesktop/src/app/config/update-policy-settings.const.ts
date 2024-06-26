import { isPackaged } from "../os-info/is-packaged";
import { UpdatePolicyIpcKey } from "./update-policy-ipc-key.const";
/**
 * Is the feature enabled
 */
export const UpdatePolicySettings = `${UpdatePolicyIpcKey}:${isPackaged().toString()}`;

const UpdatePolicyLastCheckedDate = `UPDATE_POLICY_LAST_CHECKED_DATE`;

/**
 * When is it checked for the last time
 */
export const UpdatePolicyLastCheckedDateSettings = `${UpdatePolicyLastCheckedDate}:${isPackaged().toString()}`;
