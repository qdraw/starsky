export interface IBatchRenameItem {
  sourceFilePath: string;
  targetFilePath: string;
  relatedFilePaths: string[];
  sequenceNumber: number;
  hasError: boolean;
  errorMessage?: string;
}
