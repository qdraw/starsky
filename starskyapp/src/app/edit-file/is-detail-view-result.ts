import { IGetNetRequestResponse } from "../net-request/get-net-request";

export function IsDetailViewResult(result: IGetNetRequestResponse) {
  return (
    result.statusCode === 200 &&
    result.data &&
    result.data.fileIndexItem &&
    result.data.fileIndexItem.status &&
    result.data.fileIndexItem.collectionPaths &&
    result.data.fileIndexItem.sidecarExtensionsList &&
    (result.data.fileIndexItem.status === "Ok" ||
      result.data.fileIndexItem.status === "Default")
  );
}
