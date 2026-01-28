import { IFileIndexItem } from "./IFileIndexItem";

export interface ITimezone {
  id: string;
  displayName: string;
}

export interface ITimezoneShiftRequest {
  recordedTimezone: string;
  correctTimezone: string;
}

export interface IOffsetShiftRequest {
  year: number;
  month: number;
  day: number;
  hour: number;
  minute: number;
  second: number;
}

export interface ITimezoneShiftResult {
  success: boolean;
  originalDateTime: string;
  correctedDateTime: string;
  delta: string;
  warning: string;
  error: string;
  fileIndexItem: IFileIndexItem;
}
