import React, { memo, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import MenuOption from "../../atoms/menu-option/menu-option";
import Notification, { NotificationType } from "../../atoms/notification/notification";

interface IMenuOptionDesktopEditorOpenSingleProps {
  subPath: string;
  collections: boolean;
  isReadOnly: boolean;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

export async function OpenDesktopSingle(
  subPath: string,
  collections: boolean,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  messageDesktopEditorUnableToOpen: string,
  isReadOnly: boolean
) {
  if (isReadOnly) {
    return;
  }
  const urlOpen = new UrlQuery().UrlApiDesktopEditorOpen();

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", subPath);
  bodyParams.append("collections", collections.toString());

  const openDesktopResult = await FetchPost(urlOpen, bodyParams.toString());
  if (openDesktopResult.statusCode >= 300) {
    setIsError(messageDesktopEditorUnableToOpen);
  }
}

const MenuOptionDesktopEditorOpenSingle: React.FunctionComponent<IMenuOptionDesktopEditorOpenSingleProps> =
  memo(({ subPath, collections, isReadOnly }) => {
    // Check API to know if feature is needed!
    const featuresResult = useFetch(new UrlQuery().UrlApiFeaturesAppSettings(), "get");
    const dataFeatures = featuresResult?.data as IEnvFeatures | undefined;

    // Get language keys
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageDesktopEditorUnableToOpen = language.key(
      localization.MessageDesktopEditorUnableToOpen
    );

    // for showing a notification
    const [isError, setIsError] = useState("");

    /**
     * Open editor with keys
     */
    useHotKeys({ key: "e", ctrlKeyOrMetaKey: true }, () => {
      OpenDesktopSingle(
        subPath,
        collections,
        setIsError,
        MessageDesktopEditorUnableToOpen,
        isReadOnly
      ).then(() => {
        // do nothing
      });
    });

    return (
      <>
        {isError !== "" ? (
          <Notification callback={() => setIsError("")} type={NotificationType.danger}>
            {isError}
          </Notification>
        ) : null}

        {dataFeatures?.openEditorEnabled === true ? (
          <MenuOption
            isReadOnly={isReadOnly}
            testName={"menu-option-desktop-editor-open"}
            onClickKeydown={() =>
              OpenDesktopSingle(
                subPath,
                collections,
                setIsError,
                MessageDesktopEditorUnableToOpen,
                isReadOnly
              )
            }
            localization={localization.MessageDesktopEditorOpenSingleFile}
          />
        ) : null}
      </>
    );
  });

export default MenuOptionDesktopEditorOpenSingle;
