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
  retry: number
) {
  // there is an async backend event triggered, sometimes there is an que
  setTimeout(() => {
    RequestNewFileHash(state, setIsLoading, dispatch).then((result) => {
      if (result === false) {
        setTimeout(() => {
          RequestNewFileHash(state, setIsLoading, dispatch).then(() => {
            // when it didn't change after two tries
            setIsLoading(false);
          });
        }, 7000);
      }
    });
  }, 3000);

  console.log(retry);
}
