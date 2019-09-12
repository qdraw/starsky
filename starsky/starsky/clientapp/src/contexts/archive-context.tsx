
import * as React from 'react';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIRelativeObjects, PageType } from '../interfaces/IDetailView';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';

const ArchiveContext = React.createContext<IContext>({} as IContext)
type IContext = {
  state: State,
  dispatch: React.Dispatch<Action>,
}

type ReactNodeProps = { children: React.ReactNode }
type Action = {
  type: 'update', tags?: string, colorclass?: number,
  description?: string, title?: string, append?: boolean, select: string[]
} | { type: 'reset', payload: IArchiveProps } | { type: 'add' } |
{ type: 'replace', fieldName: string, files: string[], from: string, to: string } | {
  type: 'remove', filesList: string[]
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
}

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

      console.log(deletedFilesCount);

      // to update the total results
      var collectionsCount = state.collectionsCount - deletedFilesCount;

      return { ...state, collectionsCount: collectionsCount, lastUpdated: new Date() };
    case "replace":
      // files == subpath style not only the name (/dir/file.jpg)
      var { files, fieldName, from, to } = action;

      state.fileIndexItems.forEach(item => {
        if (files.indexOf(item.filePath) === -1) return;

        var replaceRegex = new RegExp(from, "g");
        switch (fieldName.toLowerCase()) {
          case 'tags':
            if (!item.tags) break;
            item.tags = item.tags.replace(replaceRegex, to);
            break;
          default:
            break;
        }
        item.lastEdited = new Date().toISOString();

      });

      console.log(state.fileIndexItems);


      return { ...state, lastUpdated: new Date() };
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
    case "reset":
      return action.payload;
    case "add":
      state.fileIndexItems.push(newIFileIndexItem());
      return { ...state, subPath: "/" };
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

