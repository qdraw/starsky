import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import {
  IFileIndexItem,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import FetchPost from "../../../shared/fetch-post";

const CastFileIndexItem = (element: any): IFileIndexItem => {
  var uploadFileObject = newIFileIndexItem();
  uploadFileObject.fileHash = element.fileHash;
  uploadFileObject.filePath = element.filePath;
  uploadFileObject.isDirectory = false;
  uploadFileObject.fileName = element.fileName;
  uploadFileObject.lastEdited = new Date().toISOString();
  uploadFileObject.status = element.status;
  return uploadFileObject;
};

export function PostSingleFormData(
  endpoint: string,
  folderPath: string | undefined,
  inputFilesList: File[],
  index: number,
  outputUploadFilesList: IFileIndexItem[],
  callBackWhenReady: (result: IFileIndexItem[]) => void,
  setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
) {
  var formData = new FormData();

  if (inputFilesList.length === index) {
    setNotificationStatus("");
    callBackWhenReady(outputUploadFilesList);
    return;
  }

  setNotificationStatus(
    `Uploading ${index + 1}/${inputFilesList.length} ${
      inputFilesList[index].name
    }`
  );

  if (inputFilesList[index].size / 1024 / 1024 > 250) {
    outputUploadFilesList.push({
      filePath: inputFilesList[index].name,
      fileName: inputFilesList[index].name,
      status: IExifStatus.ServerError
    } as IFileIndexItem);
    next(
      endpoint,
      folderPath,
      inputFilesList,
      index,
      outputUploadFilesList,
      callBackWhenReady,
      setNotificationStatus
    );
    return;
  }

  formData.append("file", inputFilesList[index]);

  FetchPost(endpoint, formData, "post", { to: folderPath }).then((response) => {
    new ProcessResponse(endpoint).Run(
      response,
      folderPath,
      inputFilesList,
      index,
      outputUploadFilesList,
      callBackWhenReady,
      setNotificationStatus
    );
  });
}

class ProcessResponse {
  private endpoint: string;
  constructor(endpoint: string) {
    this.endpoint = endpoint;
  }
  public Run(
    response: IConnectionDefault,
    folderPath: string | undefined,
    inputFilesList: File[],
    index: number,
    outputUploadFilesList: IFileIndexItem[],
    callBackWhenReady: (result: IFileIndexItem[]) => void,
    setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
  ) {
    // When data is failed
    if (!response.data) {
      outputUploadFilesList.push({
        filePath: inputFilesList[index].name,
        fileName: inputFilesList[index].name,
        status: IExifStatus.ServerError
      } as IFileIndexItem);

      next(
        this.endpoint,
        folderPath,
        inputFilesList,
        index,
        outputUploadFilesList,
        callBackWhenReady,
        setNotificationStatus
      );
      return;
    }

    // Success
    Array.from(response.data).forEach((dataItem: any) => {
      if (!dataItem) {
        outputUploadFilesList.push({
          filePath: inputFilesList[index].name,
          fileName: inputFilesList[index].name,
          status: IExifStatus.ServerError
        } as IFileIndexItem);
      } else if (
        dataItem.fileIndexItem &&
        (dataItem.status as IExifStatus) !== IExifStatus.Ok
      ) {
        outputUploadFilesList.push(CastFileIndexItem(dataItem.fileIndexItem));
      } else if (
        !dataItem.fileIndexItem &&
        (dataItem.status as IExifStatus) !== IExifStatus.Ok
      ) {
        // when `/import` already existing item
        outputUploadFilesList.push({
          filePath: dataItem.filePath,
          fileName: inputFilesList[index].name,
          isDirectory: false,
          fileHash: dataItem.fileHash,
          status: dataItem.status
        } as IFileIndexItem);
      } else {
        dataItem.fileIndexItem.lastEdited = new Date().toISOString();
        outputUploadFilesList.push(dataItem.fileIndexItem);
      }
    });

    next(
      this.endpoint,
      folderPath,
      inputFilesList,
      index,
      outputUploadFilesList,
      callBackWhenReady,
      setNotificationStatus
    );
  }
}

function next(
  endpoint: string,
  folderPath: string | undefined,
  inputFilesList: File[],
  index: number,
  outputUploadFilesList: IFileIndexItem[],
  callBackWhenReady: (result: IFileIndexItem[]) => void,
  setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
): void {
  index++;
  PostSingleFormData(
    endpoint,
    folderPath,
    inputFilesList,
    index,
    outputUploadFilesList,
    callBackWhenReady,
    setNotificationStatus
  );
}
