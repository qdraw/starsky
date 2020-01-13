import React from 'react';
import { IDetailView, IRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem, Orientation } from '../interfaces/IFileIndexItem';

const DetailViewContext = React.createContext<IContext>({} as IContext)

type State = IDetailView
type ReactNodeProps = { children: React.ReactNode }

const initialState: State = {
  breadcrumb: [],
  fileIndexItem: newIFileIndexItem(),
  relativeObjects: {} as IRelativeObjects,
  subPath: "/",
  pageType: PageType.DetailView,
  colorClassFilterList: [],
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
} |
{
  type: 'reset',
  payload: IDetailView
};

type IContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}
export function detailviewReducer(state: State, action: Action): State {
  switch (action.type) {
    case "remove":
      var { tags } = action;
      if (tags && state.fileIndexItem.tags !== undefined) state.fileIndexItem.tags = state.fileIndexItem.tags.replace(tags, "");
      // Need to update otherwise other events are not triggerd
      return { ...state, lastUpdated: new Date() };
    case "append":
      /* eslint-disable-next-line no-redeclare */
      var { tags } = action;
      if (tags) state.fileIndexItem.tags += "," + tags;
      // Need to update otherwise other events are not triggerd
      return { ...state, lastUpdated: new Date() };
    case "update":
      /* eslint-disable-next-line no-redeclare */
      var { tags, description, title, status, colorclass, fileHash, orientation, lastEdited } = action;

      if (tags) state.fileIndexItem.tags = tags;
      if (description) state.fileIndexItem.description = description;
      if (title) state.fileIndexItem.title = title;
      if (colorclass) state.fileIndexItem.colorClass = colorclass;
      if (status) state.fileIndexItem.status = status;
      if (fileHash) state.fileIndexItem.fileHash = fileHash;
      if (orientation) state.fileIndexItem.orientation = orientation;
      if (lastEdited) state.fileIndexItem.lastEdited = lastEdited;

      // Need to update otherwise other events are not triggerd
      return { ...state, lastUpdated: new Date() };
    case "reset":
      return action.payload;
  }
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
