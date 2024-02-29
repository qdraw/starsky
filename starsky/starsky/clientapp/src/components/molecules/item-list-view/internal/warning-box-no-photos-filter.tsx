import React, { memo } from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { PageType } from "../../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { UrlQuery } from "../../../../shared/url/url-query.ts";

interface IWarningBoxNoPhotosFilterProps {
  pageType: PageType | undefined;
  subPath: string | undefined;
  items: IFileIndexItem[];
  colorClassUsage: number[];
}

export const WarningBoxNoPhotosFilter: React.FunctionComponent<IWarningBoxNoPhotosFilterProps> =
  memo(({ pageType, subPath, items, colorClassUsage }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);

    const MessageNoPhotosInFolder = language.key(localization.MessageNoPhotosInFolder);
    const MessageItemsOutsideFilter = language.key(localization.MessageItemsOutsideFilter);
    const MessageReadMoreHere = language.key(localization.MessageReadMoreHere);
    const MessageNewUserNoPhotosInFolder = language.key(
      localization.MessageNewUserNoPhotosInFolder
    );

    let warningBox = null;

    if (pageType !== PageType.Loading && subPath !== "/" && items.length === 0) {
      if (colorClassUsage.length >= 1) {
        warningBox = (
          <div
            className="warning-box warning-box--left"
            data-test="list-view-message-items-outside-filter"
          >
            {MessageItemsOutsideFilter}
          </div>
        );
      } else {
        warningBox = (
          <div className="warning-box" data-test="list-view-no-photos-in-folder">
            {MessageNoPhotosInFolder}
          </div>
        );
      }
    }

    // only on the home page there is a link to the preferences page
    if (pageType !== PageType.Loading && subPath === "/" && items.length === 0) {
      warningBox = (
        <a
          className="warning-box"
          href={new UrlQuery().UrlPreferencesPage()}
          data-test="list-view-no-photos-in-folder"
        >
          {MessageNewUserNoPhotosInFolder}{" "}
          <a target="_blank" href={new UrlQuery().DocsGettingStartedFirstSteps()} rel="noreferrer">
            {MessageReadMoreHere}.
          </a>{" "}
          {MessageNoPhotosInFolder}
        </a>
      );
    }

    return <>{warningBox}</>;
  });
