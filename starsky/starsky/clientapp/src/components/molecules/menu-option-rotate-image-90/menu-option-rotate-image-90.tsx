import React, { Dispatch, memo } from "react";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import MenuOption from "../../atoms/menu-option/menu-option.tsx";
import FetchGet from "../../../shared/fetch-get.ts";
import { CastToInterface } from "../../../shared/cast-to-interface.ts";
import { Orientation } from "../../../interfaces/IFileIndexItem.ts";
import { IDetailView } from "../../../interfaces/IDetailView.ts";
import { DetailViewAction } from "../../../contexts/detailview-context.tsx";

interface IMenuOptionMenuOptionRotateImage90Props {
  state: IDetailView;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  dispatch: Dispatch<DetailViewAction>;
  isMarkedAsDeleted: boolean;
  isReadOnly: boolean;
}

/**
 * Used from DetailView
 */
const MenuOptionRotateImage90: React.FunctionComponent<IMenuOptionMenuOptionRotateImage90Props> =
  memo(({ state, setIsLoading, dispatch, isMarkedAsDeleted, isReadOnly }) => {
    /**
     * Create body params to do url queries
     */
    function newBodyParams(): URLSearchParams {
      const bodyParams = new URLSearchParams();
      bodyParams.set("f", state.subPath);
      return bodyParams;
    }

    /**
     * Checks if the hash is changes and update Context:  orientation + fileHash
     */
    async function requestNewFileHash(): Promise<boolean | null> {
      const resultGet = await FetchGet(
        new UrlQuery().UrlIndexServerApi({ f: state.subPath })
      );
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
      if (media.fileIndexItem.fileHash === state.fileIndexItem.fileHash)
        return false;

      dispatch({
        type: "update",
        orientation,
        fileHash: media.fileIndexItem.fileHash,
        filePath: media.fileIndexItem.filePath
      });
      setIsLoading(false);
      return true;
    }

    /**
     * Update the rotation status
     */
    async function rotateImage90() {
      if (isMarkedAsDeleted || isReadOnly) return;
      setIsLoading(true);

      const bodyParams = newBodyParams();
      bodyParams.set("rotateClock", "1");
      const resultPost = await FetchPost(
        new UrlQuery().UrlUpdateApi(),
        bodyParams.toString()
      );
      if (resultPost.statusCode !== 200) {
        console.error(resultPost);
        return;
      }

      // there is an async backend event triggered, sometimes there is an que
      setTimeout(() => {
        requestNewFileHash().then((result) => {
          if (result === false) {
            setTimeout(() => {
              requestNewFileHash().then(() => {
                // when it didn't change after two tries
                setIsLoading(false);
              });
            }, 7000);
          }
        });
      }, 3000);
    }

    return (
      <MenuOption
        isReadOnly={isReadOnly}
        onClickKeydown={rotateImage90}
        localization={localization.MessageRotateToRight}
        testName="rotate"
      />
    );
  });

export default MenuOptionRotateImage90;
