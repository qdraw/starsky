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
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import SwitchButton from "../../atoms/switch-button/switch-button";

const defaultEditorApplication = {
  imageFormats: [ImageFormat.jpg, ImageFormat.png, ImageFormat.bmp, ImageFormat.tiff]
} as IAppSettingsDefaultEditorApplication;

async function updateDefaultEditorPhotos(
  event: ChangeEvent<HTMLDivElement>,
  setIsMessage: React.Dispatch<React.SetStateAction<string>>,
  MessageSwitchButtonDesktopCollectionsUpdateError: string,
  MessageSwitchButtonDesktopCollectionsUpdateSuccess: string,
  defaultDesktopEditor?: IAppSettingsDefaultEditorApplication[]
) {
  if (!defaultDesktopEditor) {
    setIsMessage("FAIL");
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

  const imageDefaultEditor = appSettings?.defaultDesktopEditor.find(
    (p) => p.imageFormats.includes(ImageFormat.jpg) || p.imageFormats.includes(ImageFormat.tiff)
  );

  const language = new Language(settings.language);
  const MessageSwitchButtonDesktopCollectionsRawOn = language.key(
    localization.MessageSwitchButtonDesktopCollectionsRawOn
  );
  const MessageSwitchButtonDesktopCollectionsJpegDefaultOff = language.key(
    localization.MessageSwitchButtonDesktopCollectionsJpegDefaultOff
  );
  const MessageSwitchButtonDesktopApplication = language.key(
    localization.MessageSwitchButtonDesktopApplication
  );
  const MessageSwitchButtonDesktopApplicationDescription = language.key(
    localization.MessageSwitchButtonDesktopApplicationDescription
  );
  const MessageAppSettingDefaultEditorPhotos = language.key(
    localization.MessageAppSettingDefaultEditorPhotos
  );
  const MessageAppSettingDefaultEditorPhotosDescription = language.key(
    localization.MessageAppSettingDefaultEditorPhotosDescription
  );

  const MessageSwitchButtonDesktopCollectionsUpdateError = language.key(
    localization.MessageSwitchButtonDesktopCollectionsUpdateError
  );

  const MessageSwitchButtonDesktopCollectionsUpdateSuccess = language.key(
    localization.MessageSwitchButtonDesktopCollectionsUpdateSuccess
  );

  // for showing a notification
  const [isMessage, setIsMessage] = useState("");

  return (
    <>
      <div className="content--subheader">{MessageSwitchButtonDesktopApplication}</div>
      <div className="content--text no-left-padding">
        <p>{MessageSwitchButtonDesktopApplicationDescription}</p>

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

        <h3>{MessageAppSettingDefaultEditorPhotos}</h3>
        <p>{MessageAppSettingDefaultEditorPhotosDescription} </p>
        <FormControl
          spellcheck={true}
          onBlur={(value) =>
            updateDefaultEditorPhotos(
              value,
              setIsMessage,
              MessageSwitchButtonDesktopCollectionsUpdateError,
              MessageSwitchButtonDesktopCollectionsUpdateSuccess,
              appSettings?.defaultDesktopEditor
            )
          }
          name="tags"
          contentEditable={true}
        >
          {imageDefaultEditor?.applicationPath}
        </FormControl>
      </div>
    </>
  );
};

export default PreferencesAppSettingsDesktop;
