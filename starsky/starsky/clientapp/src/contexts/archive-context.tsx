
import * as React from 'react';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

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
} |
{
  type: 'add',
  add: Array<IFileIndexItem>
}

type State = IArchiveProps

const initialState: State = {
  fileIndexItems: [],
  subPath: "/",
  relativeObjects: newIRelativeObjects(),
  breadcrumb: [],
  collectionsCount: 0,
  colorClassActiveList: [],
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
          // colorclass = 0 ==> colorless/no-color
          if (colorclass !== undefined && colorclass !== -1) state.fileIndexItems[index].colorClass = colorclass;
          state.fileIndexItems[index].lastEdited = new Date().toISOString();
        }
      });

      // Need to update otherwise other events are not triggerd
      return { ...state, lastUpdated: new Date() };
    case "reset-url-change":

      // for search / trash pages
      if ((action.payload.pageType === PageType.Search || action.payload.pageType === PageType.Trash) &&
        CombineSearchQueryAndPageNumber(state) !== CombineSearchQueryAndPageNumber(action.payload)
      ) {
        console.log('running dispatch (search/trash)', CombineSearchQueryAndPageNumber(state), CombineSearchQueryAndPageNumber(action.payload));
        return action.payload;
      }

      // for archive pages
      if (action.payload.pageType === PageType.Archive && (
        CombineArchive(state) !== CombineArchive(action.payload) ||
        action.payload.subPath === "/") // for home
      ) {
        console.log('running dispatch (a)', CombineArchive(state), CombineArchive(action.payload));
        return action.payload;
      }
      return state;

    case "force-reset":
      return action.payload;

    case "add":
      var concattedFileIndexItems = state.fileIndexItems.concat(action.add);
      var fileIndexItems = concattedFileIndexItems
        .sort((a, b) => (a.filePath > b.filePath) ? 1 : -1) // sort on filePath
        .filter((v, i, a) => a.findIndex(t => (t.filePath === v.filePath)) === i) // duplicate check

      return { ...state, fileIndexItems, lastUpdated: new Date() };
  }
}

function CombineArchive(payload: IArchiveProps): string {
  return `${payload.subPath}${payload.colorClassActiveList ? payload.colorClassActiveList.toString() : null}`;
}

function CombineSearchQueryAndPageNumber(payload: IArchiveProps): string {
  return `${payload.searchQuery}${payload.pageNumber}`;
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