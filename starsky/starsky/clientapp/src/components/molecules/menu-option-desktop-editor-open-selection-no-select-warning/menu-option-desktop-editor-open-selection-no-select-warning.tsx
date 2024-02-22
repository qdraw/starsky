import { memo } from "react";
import useFetch from "../../../hooks/use-fetch";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import { UrlQuery } from "../../../shared/url-query";

interface IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps {
  select: string[];
  isReadonly: boolean;
}

const MenuOptionDesktopEditorOpenSelectionNoSelectWarning: React.FunctionComponent<IMenuOptionDesktopEditorOpenSelectionNoSelectWarningProps> =
  memo(({ select, isReadonly }) => {
    // Check API to know if feature is needed!
    const featuresResult = useFetch(new UrlQuery().UrlApiFeaturesAppSettings(), "get");
    const dataFeatures = featuresResult?.data as IEnvFeatures | undefined;

    /**
     * Open editor with keys -  command + e
     */
    useHotKeys({ key: "e", ctrlKeyOrMetaKey: true }, () => {
      if (dataFeatures?.openEditorEnabled !== true || isReadonly || select.length >= 1) return;
    });

    return <></>;
  });

export default MenuOptionDesktopEditorOpenSelectionNoSelectWarning;
