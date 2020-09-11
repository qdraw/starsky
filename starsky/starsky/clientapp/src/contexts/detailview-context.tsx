import React from 'react';
import { IDetailView, IRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem, Orientation } from '../interfaces/IFileIndexItem';
import { IUrl } from '../interfaces/IUrl';
import { FileListCache } from '../shared/filelist-cache';

const DetailViewContext = React.createContext<IDetailViewContext>({} as IDetailViewContext)

type ReactNodeProps = { children: React.ReactNode }

const initialState: IDetailView = {
  breadcrumb: [],
  fileIndexItem: newIFileIndexItem(),
  relativeObjects: {} as IRelativeObjects,
  subPath: "/",
  pageType: PageType.DetailView,
  colorClassActiveList: [],
  isReadOnly: false,
  dateCache: Date.now()
}

type Action = {
  type: 'append',
  tags?: string
} |
{
  type: 'remove',
  tags?: string
} |
{
  type: 'update',
  tags?: string,
  colorclass?: number,
  description?: string,
  title?: string,
  fileHash?: string,
  orientation?: Orientation,
  status?: IExifStatus,
  lastEdited?: string,
  dateTime?: string,
} |
{
  type: 'reset',
  payload: IDetailView
};

export type IDetailViewContext = {
  state: IDetailView,
  dispatch: React.Dispatch<Action>,
}
export function detailviewReducer(state: IDetailView, action: Action): IDetailView {
  switch (action.type) {
    case "remove":
      var { tags } = action;
      if (tags && state.fileIndexItem.tags !== undefined) state.fileIndexItem.tags = state.fileIndexItem.tags.replace(tags, "");
      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "append":
      /* eslint-disable-next-line no-redeclare */
      var { tags } = action;
      if (tags) state.fileIndexItem.tags += "," + tags;
      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "update":
      /* eslint-disable-next-line no-redeclare */
      var { tags, description, title, status, colorclass, fileHash, orientation, lastEdited, dateTime } = action;
      if (tags !== undefined) state.fileIndexItem.tags = tags;
      if (description !== undefined) state.fileIndexItem.description = description;
      if (title !== undefined) state.fileIndexItem.title = title;
      if (colorclass) state.fileIndexItem.colorClass = colorclass;
      if (status) state.fileIndexItem.status = status;
      if (fileHash) state.fileIndexItem.fileHash = fileHash;
      if (orientation) state.fileIndexItem.orientation = orientation;
      if (lastEdited) state.fileIndexItem.lastEdited = lastEdited;
      if (dateTime) state.fileIndexItem.dateTime = dateTime;

      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "reset":
      // this is triggert a lot when loading a page
      return action.payload;
  }
}

/**
 * Update the cache based on the keys
 */
function updateCache(stateLocal: IDetailView): IDetailView {
  if (!stateLocal.fileIndexItem) {
    return stateLocal;
  }
  var urlObject = { f: stateLocal.subPath, colorClass: stateLocal.colorClassActiveList, collections: stateLocal.collections } as IUrl;
  new FileListCache().CacheSetObject(urlObject, { ...stateLocal });
  return stateLocal;
}

function DetailViewContextProvider({ children }: ReactNodeProps) {
  // [A]
  let [state, dispatch] = React.useReducer(detailviewReducer, initialState);
  let value1 = { state, dispatch };

  // [B]
  return (
    <DetailViewContext.Provider value={value1}>{children}</DetailViewContext.Provider>
  );
}

let DetailViewContextConsumer = DetailViewContext.Consumer;

export { DetailViewContext, DetailViewContextProvider, DetailViewContextConsumer };

export const useDetailViewContext = () => React.useContext(DetailViewContext);
