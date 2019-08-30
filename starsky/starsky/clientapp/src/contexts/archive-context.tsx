
import * as React from 'react';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIRelativeObjects } from '../interfaces/IDetailView';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';

const ArchiveContext = React.createContext<IContext>({} as IContext)
type IContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}


type CountProviderProps = { children: React.ReactNode }
type Action = {
  type: 'update', tags: string,
  description: string, title: string, append: boolean, select: string[]
} | { type: 'reset', payload: IArchiveProps } | { type: 'add' }

type State = IArchiveProps

const initialState: State = {
  fileIndexItems: [],
  subPath: "/",
  relativeObjects: newIRelativeObjects(),
  breadcrumb: [],
  collectionsCount: 0,
  colorClassFilterList: [],
  colorClassUsage: []
}

export function archiveReducer(state: State, action: Action): State {
  switch (action.type) {
    case "update":
      var updated = state;


      var { select, tags, description, title, append } = action;
      console.log(state, select);

      updated.fileIndexItems.forEach((item, index) => {
        if (select.indexOf(item.fileName) !== -1) {
          if (append) {
            // bug: duplicate keywords are added, in the api those are filtered
            if (tags) state.fileIndexItems[index].tags += ", " + tags;
            if (description) state.fileIndexItems[index].description += description;
            if (title) state.fileIndexItems[index].title += title;
          }
          else {
            if (tags) state.fileIndexItems[index].tags = tags;
            if (description) state.fileIndexItems[index].description = description;
            if (title) state.fileIndexItems[index].title = title;
          }
          state.fileIndexItems[index].lastEdited = new Date().toISOString();
        }
      });

      // Need to update otherwise other events are not triggerd
      return { ...updated, lastUpdated: new Date() };
    case "reset":
      return action.payload;
    case "add":
      state.fileIndexItems.push(newIFileIndexItem());
      return { ...state, subPath: "/" };
  }
};

function ArchiveContextProvider({ children }: CountProviderProps) {
  // [A]
  let [state, dispatch] = React.useReducer(archiveReducer, initialState);
  let value1 = { state, dispatch };

  // [B]
  return (
    <ArchiveContext.Provider value={value1}>{children}</ArchiveContext.Provider>
  );
}

let ArchiveContextConsumer = ArchiveContext.Consumer;

// [C]
export { ArchiveContext, ArchiveContextProvider, ArchiveContextConsumer };

