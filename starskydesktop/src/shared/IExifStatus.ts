export enum IExifStatus {
  Default = "Default" as any,
  ReadOnly = "ReadOnly" as any,
  Ok = "Ok" as any,
  Deleted = "Deleted" as any,
  NotFoundSourceMissing = "NotFoundSourceMissing" as any,
  ServerError = "ServerError" as any,
  IgnoredAlreadyImported = "IgnoredAlreadyImported" as any,
  FileError = "FileError" as any,
}

export default IExifStatus;
