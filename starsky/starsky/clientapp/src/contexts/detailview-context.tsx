import React from 'react';
import { IDetailView, IRelativeObjects } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';

const DetailViewContext = React.createContext<IContext>({} as IContext)

type State = IDetailView
type ReactNodeProps = { children: React.ReactNode }

const initialState: State = {
  breadcrumb: [],
  fileIndexItem: newIFileIndexItem(),
  relativeObjects: {} as IRelativeObjects,
  subPath: "/",
  status: IExifStatus.Default,
  pageType: 'DetailView',
  colorClassFilterList: [],
}

type Action = { type: 'add', tags?: string } | { type: 'remove', tags?: string } | {
  type: 'update', tags?: string, colorclass?: number,
  description?: string, title?: string, append?: boolean, status?: IExifStatus
} | { type: 'reset', payload: IDetailView };

type IContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}
export function archiveReducer(state: State, action: Action): State {
  switch (action.type) {
    case "remove":
      var updated = state;
      // updated.status
      // Need to update otherwise other events are not triggerd
      return { ...updated, lastUpdated: new Date() };
    case "add":
      var updated = state;
      // updated.status
      // Need to update otherwise other events are not triggerd
      return { ...updated, lastUpdated: new Date() };
    case "update":
      var updated = state;
      // updated.status
      // Need to update otherwise other events are not triggerd
      return { ...updated, lastUpdated: new Date() };
    case "reset":
      return action.payload;
  }
};

function DetailViewContextProvider({ children }: ReactNodeProps) {
  // [A]
  let [state, dispatch] = React.useReducer(archiveReducer, initialState);
  let value1 = { state, dispatch };

  // [B]
  return (
    <DetailViewContext.Provider value={value1}>{children}</DetailViewContext.Provider>
  );
}

let DetailViewContextConsumer = DetailViewContext.Consumer;

export { DetailViewContext, DetailViewContextProvider, DetailViewContextConsumer };

