import React, { Dispatch } from "react";
import { DetailViewAction } from "../../../../contexts/detailview-context.tsx";
import { IDetailView } from "../../../../interfaces/IDetailView.ts";
import { Orientation } from "../../../../interfaces/IFileIndexItem.ts";
import { CastToInterface } from "../../../../shared/cast-to-interface.ts";
import FetchGet from "../../../../shared/fetch/fetch-get.ts";
import { UrlQuery } from "../../../../shared/url/url-query.ts";

/**
 * Checks if the hash is changes and update Context:  orientation + fileHash
 */

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
