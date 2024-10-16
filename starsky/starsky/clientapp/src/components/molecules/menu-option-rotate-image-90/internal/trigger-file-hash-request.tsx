import { Dispatch } from "react";
import { DetailViewAction } from "../../../../contexts/detailview-context";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { Orientation } from "../../../../interfaces/IFileIndexItem";
import { CastToInterface } from "../../../../shared/cast-to-interface";
import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";

export async function RequestNewFileHash(
  state: IDetailView,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  dispatch: Dispatch<DetailViewAction>
): Promise<boolean | null> {
  const resultGet = await FetchGet(new UrlQuery().UrlIndexServerApi({ f: state.subPath }));
  if (!resultGet) return null;
  if (resultGet.statusCode !== 200) {
    console.error(resultGet);
    setIsLoading(false);
    return null;
  }
  const media = new CastToInterface().MediaDetailView(resultGet.data).data;
  const orientation = media?.fileIndexItem?.orientation
    ? media.fileIndexItem.orientation
    : Orientation.Horizontal;

  // the hash changes if you rotate an image
  if (media.fileIndexItem.fileHash === state.fileIndexItem.fileHash) return false;

  dispatch({
    type: "update",
    orientation,
    fileHash: media.fileIndexItem.fileHash,
    filePath: media.fileIndexItem.filePath
  });
  setIsLoading(false);
  return true;
}

export function TriggerFileHashRequest(
  state: IDetailView,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  dispatch: Dispatch<DetailViewAction>,
  retry: number = 0
) {
  const maxRetries = 3;
  const delay = 3000;

  const attemptRequest = (currentRetry: number) => {
    setTimeout(() => {
      RequestNewFileHash(state, setIsLoading, dispatch).then((result) => {
        if (result === false && currentRetry < maxRetries) {
          attemptRequest(currentRetry + 1);
        } else {
          setIsLoading(false);
        }
      });
    }, delay);
  };

  attemptRequest(retry);
}
