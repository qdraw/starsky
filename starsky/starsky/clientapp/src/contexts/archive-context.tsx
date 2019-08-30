
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
  description: string, title: string, select: string[]
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


      var { select, tags, description, title } = action;
      console.log(state, select);

      updated.fileIndexItems.forEach((item, index) => {
        if (select.indexOf(item.fileName) !== -1) {
          var removed = state.fileIndexItems.splice(index, 1)[0];
          removed.tags = "sdfdsf";
          removed.filePath = "sdfdsf";
          console.log(removed, index);

          updated.fileIndexItems.splice(index, 0, removed);
          // state.fileIndexItems.push(removed);

          // state.fileIndexItems[index].tags = "sdfdsf";

        }
      });

      // // if (!state.fileIndexItems) return state;
      // var index = 1;
      // updated.fileIndexItems = updated.fileIndexItems.splice(index, 1)

      updated.fileIndexItems[1].tags = "t" + action.tags;
      updated.fileIndexItems[1].description = action.tags;
      // if (state.fileIndexItems.length >= 1) {
      //   // // console.log(state);

      //   // state.fileIndexItems[1].fileHash = action.tags;
      //   // state.fileIndexItems[1].tags = action.tags;
      //   // console.log(state.fileIndexItems[1]);
      //   state.fileIndexItems.push(removed);
      // }

      // state.fileIndexItems[1].tags = "sdkfnldf";
      console.log(updated.fileIndexItems[1]);

      // subPath: "~"
      return { ...updated, subPath: Math.random().toString() };
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

