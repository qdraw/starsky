import { IArchive } from "./IArchive";
import { IDetailView } from "./IDetailView";

/**
 * In plaats van een type met alleen strings defineren we hier een
 * type met zowel een key als een "Value" (voor zover types values hebben)
 */
// let op, hij maakt hier stiekem een interface van ipv type
export interface IMediaTypes {
  DetailView: IDetailView;
  Archive: IArchive;
}

export interface IMedia<T extends keyof IMediaTypes = keyof IMediaTypes> {
  type: T;
  data: IMediaTypes[T];
}

export function newIMedia(): IMedia {
  return {} as IMedia;
}
