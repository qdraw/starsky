import { IGlobalSettings } from "../../../hooks/use-global-settings";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import localization from "../../../localization/localization.json";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language, SupportedLanguages } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";

export class ColorClassUpdateSingle {
  private isEnabled = false;
  private setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  private filePath: string;
  private collections: boolean;
  private setIsError: (value: React.SetStateAction<string>) => void;
  private language: SupportedLanguages;
  private setCurrentColorClass: (value: React.SetStateAction<number | undefined>) => void;
  private onToggle: (value: number) => void;
  private clearAfter: boolean | undefined;

  constructor(
    isEnabled: boolean,
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
    filePath: string,
    collections: boolean,
    setIsError: (value: React.SetStateAction<string>) => void,
    settings: IGlobalSettings,
    setCurrentColorClass: (value: React.SetStateAction<number | undefined>) => void,
    onToggle: (value: number) => void,
    clearAfter: boolean | undefined
  ) {
    this.isEnabled = isEnabled;
    this.setIsLoading = setIsLoading;
    this.filePath = filePath;
    this.collections = collections;
    this.setIsError = setIsError;
    this.language = settings.language;
    this.setCurrentColorClass = setCurrentColorClass;
    this.onToggle = onToggle;
    this.clearAfter = clearAfter;
  }

  private getMessageWriteErrorReadOnly() {
    return new Language(this.language).key(localization.MessageWriteErrorReadOnly);
  }

  public Update(colorClass: number) {
    if (!this.isEnabled) return;

    this.setIsLoading(true);
    const updateApiUrl = new UrlQuery().UrlUpdateApi();

    const bodyParams = new URLSearchParams();
    bodyParams.append("f", this.filePath);
    bodyParams.append("colorclass", colorClass.toString());
    bodyParams.append("collections", this.collections.toString());

    FetchPost(updateApiUrl, bodyParams.toString()).then((anyData) => {
      const result = new CastToInterface().InfoFileIndexArray(anyData.data);
      this.setIsLoading(false);
      if (
        !result ||
        result.find((item) => {
          return item.status === IExifStatus.ReadOnly;
        })
      ) {
        this.setIsError(this.getMessageWriteErrorReadOnly());
        return;
      }
      this.setCurrentColorClass(colorClass);
      this.onToggle(colorClass);
    });

    if (!this.clearAfter) return;

    setTimeout(() => {
      this.setCurrentColorClass(undefined);
    }, 1000);
  }
}
