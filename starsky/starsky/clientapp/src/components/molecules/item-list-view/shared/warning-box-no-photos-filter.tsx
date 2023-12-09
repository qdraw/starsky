import { memo } from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { PageType } from "../../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";

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

    const MessageNoPhotosInFolder = language.key(
      localization.MessageNoPhotosInFolder
    );
    const MessageItemsOutsideFilter = language.key(
      localization.MessageItemsOutsideFilter
    );

    return (
      <>
        {pageType !== PageType.Loading &&
        subPath !== "/" &&
        items.length === 0 ? (
          colorClassUsage.length >= 1 ? (
            <div
              className="warning-box warning-box--left"
              data-test="list-view-message-items-outside-filter"
            >
              {MessageItemsOutsideFilter}
            </div>
          ) : (
            <div
              className="warning-box"
              data-test="list-view-no-photos-in-folder"
            >
              {MessageNoPhotosInFolder}
            </div>
          )
        ) : null}
      </>
    );
  });
