import { memo, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import { UrlQuery } from "../../../shared/url-query";
import Notification, { NotificationType } from "../../atoms/notification/notification";

interface IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps {
  select: string[];
  isReadOnly: boolean;
}

const MenuOptionDesktopEditorOpenSelectionNoSelectWarning: React.FunctionComponent<IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps> =
  memo(({ select, isReadOnly }) => {
    // Check API to know if feature is needed!
    const featuresResult = useFetch(new UrlQuery().UrlApiFeaturesAppSettings(), "get");
    const dataFeatures = featuresResult?.data as IEnvFeatures | undefined;

    // for showing a notification
    const [isError, setIsError] = useState("");

    /**
     * Open editor with keys -  command + e
     */
    useHotKeys({ key: "e", ctrlKeyOrMetaKey: true }, () => {
      if (dataFeatures?.openEditorEnabled !== true || isReadOnly || select.length >= 1) {
        return;
      }
      setIsError("select first");
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
