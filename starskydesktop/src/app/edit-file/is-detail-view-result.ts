/* eslint-disable @typescript-eslint/no-unsafe-member-access  */
/* eslint-disable @typescript-eslint/no-unsafe-argument */
import { IGetNetRequestResponse } from "../net-request/get-net-request";

export function IsDetailViewResult(result: IGetNetRequestResponse) {
  return (
    result.statusCode === 200
    && result.data?.fileIndexItem?.status
    && result.data?.fileIndexItem?.collectionPaths
    && result.data?.fileIndexItem?.sidecarExtensionsList
    && ["Ok", "OkAndSame", "Default"].includes(result.data.fileIndexItem.status)
  );
}
