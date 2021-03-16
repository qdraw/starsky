import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { MoreMenuEventCloseConst } from "../more-menu/more-menu";
import { PostSingleFormData } from "./post-single-form-data";

export class UploadFiles {
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  setNotificationStatus: React.Dispatch<React.SetStateAction<string>>;
  propsEndpoint: string;
  propsFolderPath: string | undefined;
  propsCallback: ((result: IFileIndexItem[]) => void) | undefined;

  /**
   *
   */
  constructor(
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
    setNotificationStatus: React.Dispatch<React.SetStateAction<string>>,
    propsEndpoint: string,
    propsFolderPath: string | undefined,
    propsCallback: ((result: IFileIndexItem[]) => void) | undefined
  ) {
    this.setIsLoading = setIsLoading;
    this.setNotificationStatus = setNotificationStatus;
    this.propsEndpoint = propsEndpoint;
    this.propsFolderPath = propsFolderPath;
    this.propsCallback = propsCallback;
  }

  public uploadFiles(files: FileList) {
    /**
     * Pushing content to the server
     * @param files FileList
     */
    // only needed for the more menu
    window.dispatchEvent(
      new CustomEvent(MoreMenuEventCloseConst, { bubbles: false })
    );

    var filesList = Array.from(files);

    const { length } = filesList;
    if (length === 0) {
      return false;
    }
    console.log("Files: ", files);

    this.setIsLoading(true);

    PostSingleFormData(
      this.propsEndpoint,
      this.propsFolderPath,
      filesList,
      0,
      newIFileIndexItemArray(),
      (result) => {
        this.setIsLoading(false);
        if (this.propsCallback) {
          this.propsCallback(result);
        }
      },
      this.setNotificationStatus
    );
  }
}
