import React, { ChangeEvent, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import { IAppSettingsDefaultEditorApplication } from "../../../interfaces/IAppSettingsDefaultEditorApplication";
import { RawJpegMode } from "../../../interfaces/ICollectionsOpenType";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";
import SwitchButton from "../../atoms/switch-button/switch-button";

const defaultEditorApplication = {
  imageFormats: [
    ImageFormat.jpg,
    ImageFormat.png,
    ImageFormat.bmp,
    ImageFormat.tiff,
    ImageFormat.webp
  ]
} as IAppSettingsDefaultEditorApplication;

export async function UpdateDefaultEditorPhotos(
  event: ChangeEvent<HTMLDivElement>,
  setIsMessage: React.Dispatch<React.SetStateAction<string>>,
  MessageSwitchButtonDesktopCollectionsUpdateError: string,
  MessageSwitchButtonDesktopCollectionsUpdateSuccess: string,
  defaultDesktopEditor?: IAppSettingsDefaultEditorApplication[]
) {
  if (!defaultDesktopEditor) {
    setIsMessage(MessageSwitchButtonDesktopCollectionsUpdateError);
    return;
  }
  const bodyParams = new URLSearchParams();

  defaultEditorApplication.applicationPath = event.target.innerText;
  const index = defaultDesktopEditor.findIndex(
    (p) => p.imageFormats.includes(ImageFormat.jpg) || p.imageFormats.includes(ImageFormat.tiff)
  );
  if (index === -1) {
    defaultDesktopEditor.push(defaultEditorApplication);
  } else {
    defaultDesktopEditor[index] = defaultEditorApplication;
  }

  defaultDesktopEditor.forEach((editorApp, index) => {
    defaultEditorApplication.imageFormats.forEach((imageFormat, idx) => {
      bodyParams.append(
        `DefaultDesktopEditor[${index}].ImageFormats[${idx}]`,
        imageFormat.toString()
      );
    });
    bodyParams.append(`DefaultDesktopEditor[${index}].ApplicationPath`, editorApp.applicationPath);
  });

  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  if (result.statusCode != 200) {
    setIsMessage(MessageSwitchButtonDesktopCollectionsUpdateError);
    return;
  }
  setIsMessage(MessageSwitchButtonDesktopCollectionsUpdateSuccess);
}

export async function ToggleCollections(
  value: boolean,
  setIsMessage: React.Dispatch<React.SetStateAction<string>>,
  MessageSwitchButtonDesktopCollectionsUpdateError: string,
  MessageSwitchButtonDesktopCollectionsUpdateSuccess: string,
  appSettings: IAppSettings | null
) {
  const desktopCollectionsOpen = value ? RawJpegMode.Raw : RawJpegMode.Jpeg;

  const bodyParams = new URLSearchParams();
  bodyParams.set("desktopCollectionsOpen", desktopCollectionsOpen.toString());

  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  if (result.statusCode != 200 || !appSettings) {
    setIsMessage(MessageSwitchButtonDesktopCollectionsUpdateError);
    return;
  }
  // to avoid re-render issues to display message
  appSettings.desktopCollectionsOpen = desktopCollectionsOpen;
  setIsMessage(MessageSwitchButtonDesktopCollectionsUpdateSuccess);
}

const PreferencesAppSettingsDesktop: React.FunctionComponent = () => {
  // Get AppSettings from backend
  const appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), "get")
    ?.data as IAppSettings | null;
  // roles
  const permissionsData = useFetch(new UrlQuery().UrlAccountPermissions(), "get");

  const isAppSettingsWrite =
    Array.isArray(permissionsData?.data) &&
    permissionsData.data.includes(new UrlQuery().KeyAccountPermissionAppSettingsWrite());

  const settings = useGlobalSettings();

  const imageDefaultEditor = appSettings?.defaultDesktopEditor.find(
    (p) => p.imageFormats.includes(ImageFormat.jpg) || p.imageFormats.includes(ImageFormat.tiff)
  );

  const language = new Language(settings.language);
  const MessageSwitchButtonDesktopApplication = language.key(
    localization.MessageSwitchButtonDesktopApplication
  );
  const MessageSwitchButtonDesktopApplicationDescription = language.key(
    localization.MessageSwitchButtonDesktopApplicationDescription
  );

  // for showing a notification
  const [isMessage, setIsMessage] = useState("");

  return (
    <>
      <div className="content--subheader">{MessageSwitchButtonDesktopApplication}</div>
      <div className="content--text no-left-padding">
        <div
          data-test={
            appSettings?.useLocalDesktop
              ? "preference-app-settings-desktop-use-local-desktop-true"
              : "preference-app-settings-desktop-use-local-desktop-false"
          }
          className={
            appSettings?.useLocalDesktop ? "warning-box warning-box--optional" : "warning-box"
          }
        >
          {MessageSwitchButtonDesktopApplicationDescription}
        </div>

        {isMessage !== "" ? (
          <div
            data-test="preference-app-settings-desktop-warning-box"
            className="warning-box warning-box--optional"
          >
            {isMessage}
          </div>
        ) : null}
        <SwitchButton
          isOn={appSettings?.desktopCollectionsOpen === RawJpegMode.Raw}
          data-test="desktop-collections-open-toggle"
          isEnabled={appSettings?.useLocalDesktop && isAppSettingsWrite}
          leftLabel={language.key(localization.MessageSwitchButtonDesktopCollectionsJpegDefaultOff)}
          onToggle={(value) =>
            ToggleCollections(
              value,
              setIsMessage,
              language.key(localization.MessageSwitchButtonDesktopCollectionsRawJpegUpdateError),
              language.key(localization.MessageSwitchButtonDesktopCollectionsRawJpegUpdateSuccess),
              appSettings
            )
          }
          rightLabel={language.key(localization.MessageSwitchButtonDesktopCollectionsRawOn)}
        />
      </div>
      <div className="content--text no-left-padding">
        <h3>{language.key(localization.MessageAppSettingDefaultEditorPhotos)}</h3>
        <p>{language.key(localization.MessageAppSettingDefaultEditorPhotosDescription)} </p>
        <FormControl
          spellcheck={true}
          onBlur={(value) =>
            UpdateDefaultEditorPhotos(
              value,
              setIsMessage,
              language.key(localization.MessageSwitchButtonDesktopCollectionsUpdateError),
              language.key(localization.MessageSwitchButtonDesktopCollectionsUpdateSuccess),
              appSettings?.defaultDesktopEditor
            )
          }
          name="tags"
          contentEditable={appSettings?.useLocalDesktop === true && isAppSettingsWrite}
        >
          {imageDefaultEditor?.applicationPath}
        </FormControl>
      </div>
    </>
  );
};

export default PreferencesAppSettingsDesktop;
