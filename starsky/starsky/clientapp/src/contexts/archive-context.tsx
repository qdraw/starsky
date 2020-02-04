
import * as React from 'react';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

const ArchiveContext = React.createContext<IArchiveContext>({} as IArchiveContext)

export type IArchiveContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}

type ReactNodeProps = { children: React.ReactNode }
type Action = {
  type: 'remove',
  toRemoveFileList: string[]
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
      var { toRemoveFileList } = action;

      var deletedFilesCount = 0;
      let afterFileIndexItems = state.fileIndexItems;

      for (let index = 0; index < state.fileIndexItems.length; index++) {
        const item = state.fileIndexItems[index];

        console.log(toRemoveFileList.indexOf(item.filePath));

        if (toRemoveFileList.indexOf(item.filePath) === -1) continue;
        afterFileIndexItems.splice(index, 1);
        deletedFilesCount++
      }

      // to update the total results
      var collectionsCount = state.collectionsCount - deletedFilesCount;

      console.log(afterFileIndexItems);

      return { ...state, fileIndexItems: afterFileIndexItems, collectionsCount, lastUpdated: new Date() };
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
    case "force-reset":
      return action.payload;

    case "add":
      var filterOkCondition = (value: IFileIndexItem) => {
        return (value.status === IExifStatus.Ok || value.status === IExifStatus.Default);
      };
      var concattedFileIndexItems = state.fileIndexItems.concat(action.add);
      var fileIndexItems = [...concattedFileIndexItems].sort((a, b) => (a.filePath > b.filePath) ? 1 : -1); // sort on filePath
      fileIndexItems = fileIndexItems.filter((v, i, a) => a.findIndex(t => (t.filePath === v.filePath)) === i); // duplicate check
      fileIndexItems = fileIndexItems.filter(filterOkCondition);
      return { ...state, fileIndexItems, lastUpdated: new Date() };
  }
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