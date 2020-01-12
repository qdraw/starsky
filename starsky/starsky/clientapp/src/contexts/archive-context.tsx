
import * as React from 'react';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIRelativeObjects, PageType } from '../interfaces/IDetailView';

const ArchiveContext = React.createContext<IArchiveContext>({} as IArchiveContext)

export type IArchiveContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}

type ReactNodeProps = { children: React.ReactNode }
type Action = {
  type: 'remove',
  filesList: string[]
} |
{
  type: 'update',
  tags?: string,
  colorclass?: number,
  description?: string,
  title?: string,
  append?: boolean,
  select: string[]
} |
{
  type: 'reset-url-change',
  payload: IArchiveProps
} |
{
  type: 'force-reset',
  payload: IArchiveProps
}

type State = IArchiveProps

const initialState: State = {
  fileIndexItems: [],
  subPath: "/",
  relativeObjects: newIRelativeObjects(),
  breadcrumb: [],
  collectionsCount: 0,
  colorClassFilterList: [],
  colorClassUsage: [],
  isReadOnly: false,
  pageType: PageType.Loading
};

export function archiveReducer(state: State, action: Action): State {
  switch (action.type) {
    case "remove":
      // files == subpath style not only the name (/dir/file.jpg)
      var { filesList } = action;

      var deletedFilesCount = 0;
      state.fileIndexItems.forEach((item, index) => {
        if (filesList.indexOf(item.filePath) === -1) return;
        state.fileIndexItems.splice(index, 1);
        deletedFilesCount++
      });

      // to update the total results
      var collectionsCount = state.collectionsCount - deletedFilesCount;
      console.log('collectionsCount', collectionsCount);

      return { ...state, collectionsCount, lastUpdated: new Date() };
    case "update":

      var { select, tags, description, title, append, colorclass } = action;

      state.fileIndexItems.forEach((item, index) => {
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
          if (colorclass) state.fileIndexItems[index].colorClass = colorclass;
          state.fileIndexItems[index].lastEdited = new Date().toISOString();
        }
      });

      // Need to update otherwise other events are not triggerd
      return { ...state, lastUpdated: new Date() };
    case "reset-url-change":

      // for search / trash pages
      if (action.payload.pageType !== PageType.Archive &&
        CombineSearchQueryAndPageNumber(action.payload.searchQuery, action.payload.pageNumber) !==
        CombineSearchQueryAndPageNumber(state.searchQuery, state.pageNumber)
      ) {
        console.log('running dispatch');
        return action.payload;
      }

      // for archive pages
      if (action.payload.pageType === PageType.Archive && (action.payload.subPath !== state.subPath || action.payload.subPath === "/")) {
        console.log('running dispatch');
        return action.payload;
      }
      return state;

    case "force-reset":
      return action.payload;
  }
}

function CombineSearchQueryAndPageNumber(searchQuery: string | undefined, pageNumber: number | undefined) {
  return `${searchQuery} + ${pageNumber}`;
}

function ArchiveContextProvider({ children }: ReactNodeProps) {
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

// exporter
export const useArchiveContext = () => React.useContext(ArchiveContext)