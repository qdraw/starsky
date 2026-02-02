import { IFileIndexItem } from "./IFileIndexItem";

export interface IBatchRenameOffsetRequest {
  filePaths: string[];
  collections: boolean;
  correctionRequest: {
    year: number;
    month: number;
    day: number;
    hour: number;
    minute: number;
    second: number;
  };
}

export interface IBatchRenameTimezoneRequest {
  filePaths: string[];
  collections: boolean;
  correctionRequest: {
    recordedTimezoneId: string;
    correctTimezoneId: string;
  };
}

export interface IBatchRenameResult {
  sourceFilePath: string;
  detectedPatternDescription: string;
  originalDateTime: string;
  correctedDateTime: string;
  targetFilePath: string;
  relatedFilePaths: Record<string, unknown>[];
  offsetHours: number;
  hasError: boolean;
  errorMessage: string;
  warning: string;
  fileIndexItem: IFileIndexItem;
}
