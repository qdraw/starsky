import React, { Dispatch, memo } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context.tsx";
import { IDetailView } from "../../../interfaces/IDetailView.ts";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query.ts";
import MenuOption from "../../atoms/menu-option/menu-option.tsx";
import { TriggerFileHashRequest } from "./internal/trigger-file-hash-request.tsx";

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
     * Update the rotation status
     */
    async function rotateImage90() {
      if (isMarkedAsDeleted || isReadOnly) return;
      setIsLoading(true);

      const bodyParams = newBodyParams();
      bodyParams.set("rotateClock", "1");
      const resultPost = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
      if (resultPost.statusCode !== 200) {
        console.error(resultPost);
        return;
      }

      // there is an async backend event triggered, sometimes there is an que
      TriggerFileHashRequest(state, setIsLoading, dispatch, 3);
    }

    return (
      <MenuOption
        isReadOnly={isReadOnly}
        onClickKeydown={rotateImage90}
        localization={localization.MessageRotateToRight}
        testName="rotate" // data-test
      />
    );
  });

export default MenuOptionRotateImage90;
