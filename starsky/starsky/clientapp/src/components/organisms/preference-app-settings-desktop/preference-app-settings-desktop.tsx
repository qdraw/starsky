import React, { ChangeEvent, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import { RawJpegMode } from "../../../interfaces/ICollectionsOpenType";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import SwitchButton from "../../atoms/switch-button/switch-button";

async function updateDefaultEditorPhotos(
  event: ChangeEvent<HTMLDivElement>,
  setIsMessage: React.Dispatch<React.SetStateAction<string>>
) {
  const bodyParams = new URLSearchParams();
  bodyParams.set("desktopC1111ollectionsOpen", event.target.innerText);

  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  if (result.statusCode != 200) {
    setIsMessage("FAIL");
    return;
  }
  setIsMessage("OK");
}

async function toggleCollections(
  value: boolean,
  setIsMessage: React.Dispatch<React.SetStateAction<string>>
) {
  const desktopCollectionsOpen = value ? RawJpegMode.Raw : RawJpegMode.Jpeg;

  const bodyParams = new URLSearchParams();
  bodyParams.set("desktopCollectionsOpen", desktopCollectionsOpen.toString());

  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  if (result.statusCode != 200) {
    setIsMessage("FAIL");
    return;
  }
  setIsMessage("OK");
}

const PreferencesAppSettingsDesktop: React.FunctionComponent = () => {
  const appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), "get")
    ?.data as IAppSettings | null;

  const settings = useGlobalSettings();

  const language = new Language(settings.language);
  const MessageSwitchButtonDesktopCollectionsRawOn = language.key(
    localization.MessageSwitchButtonDesktopCollectionsRawOn
  );
  const MessageSwitchButtonDesktopCollectionsJpegDefaultOff = language.key(
    localization.MessageSwitchButtonDesktopCollectionsJpegDefaultOff
  );

  // appSettings?.desktopCollectionsOpen
  // List<AppSettingsDefaultEditorApplication> DefaultDesktopEditor
  // CollectionsOpenType.RawJpegMode DesktopCollectionsOpen

  // for showing a notification
  const [isMessage, setIsMessage] = useState("");

  return (
    <>
      <p>Desktop</p>
      Only used when using Starsky as desktop
      {isMessage !== "" ? (
        <div className="warning-box warning-box--optional">{isMessage}</div>
      ) : null}
      <SwitchButton
        isOn={appSettings?.desktopCollectionsOpen === RawJpegMode.Raw}
        data-test="desktop-collections-open-toggle"
        isEnabled={true}
        leftLabel={MessageSwitchButtonDesktopCollectionsJpegDefaultOff}
        onToggle={(value) => toggleCollections(value, setIsMessage)}
        rightLabel={MessageSwitchButtonDesktopCollectionsRawOn}
      />
      <h4>Default application to edit photos:</h4>
      <FormControl
        spellcheck={true}
        onInput={(value) => updateDefaultEditorPhotos(value, setIsMessage)}
        name="tags"
        contentEditable={true}
      ></FormControl>
    </>
  );
};

export default PreferencesAppSettingsDesktop;
