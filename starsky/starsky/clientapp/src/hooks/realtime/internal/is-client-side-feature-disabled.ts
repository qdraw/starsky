/**
 * Set an localStorage cookie when no websocket client is used
 */
export function IsClientSideFeatureDisabled(): boolean {
  return localStorage.getItem("use-sockets") === "false";
}
