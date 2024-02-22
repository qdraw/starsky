import { memo, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import Notification, { NotificationType } from "../../atoms/notification/notification";

interface IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps {
  select?: string[];
  isReadOnly: boolean;
}

const MenuOptionDesktopEditorOpenSelectionNoSelectWarning: React.FunctionComponent<IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps> =
  memo(({ select, isReadOnly }) => {
    const selectArray = select ?? [];
    // Check API to know if feature is needed!
    const featuresResult = useFetch(new UrlQuery().UrlApiFeaturesAppSettings(), "get");
    const dataFeatures = featuresResult?.data as IEnvFeatures | undefined;

    // for showing a notification
    const [isError, setIsError] = useState("");

    // Get language keys
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageItemSelectionRequired = language.key(localization.MessageItemSelectionRequired);

    /**
     * Open editor with keys -  command + e
     */
    useHotKeys({ key: "e", ctrlKeyOrMetaKey: true }, () => {
      if (dataFeatures?.openEditorEnabled !== true || isReadOnly || selectArray.length >= 1) {
        setIsError("");
        return;
      }
      setIsError(MessageItemSelectionRequired);
    });

    return (
      <>
        {isError !== "" ? (
          <Notification callback={() => setIsError("")} type={NotificationType.default}>
            {isError}
          </Notification>
        ) : null}
      </>
    );
  });

export default MenuOptionDesktopEditorOpenSelectionNoSelectWarning;
