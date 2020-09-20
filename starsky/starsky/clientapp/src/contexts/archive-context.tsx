
import * as React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { IRelativeObjects, newIRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IUrl } from '../interfaces/IUrl';
import ArrayHelper from '../shared/array-helper';
import { FileListCache } from '../shared/filelist-cache';

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
  type: 'set',
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
  pageType: PageType.Loading,
  dateCache: Date.now()
};

export function archiveReducer(state: State, action: Action): State {
  switch (action.type) {
    case "remove":
      // files == subpath style not only the name (/dir/file.jpg)
      var { toRemoveFileList } = action;

      var deletedFilesCount = 0;
      let afterFileIndexItems: IFileIndexItem[] = [];

      state.fileIndexItems.forEach(item => {
        if (toRemoveFileList.indexOf(item.filePath) === -1) {
          afterFileIndexItems.push(item);
        }
        else {
          deletedFilesCount++;
        }
      });

      // to update the total results
      var collectionsCount = state.collectionsCount - deletedFilesCount;

      return updateCache({ ...state, fileIndexItems: afterFileIndexItems, collectionsCount, lastUpdated: new Date() });
    case "update":

      var { select, tags, description, title, append, colorclass } = action;

      state.fileIndexItems.forEach((item, index) => {
        if (select.indexOf(item.fileName) !== -1) {
          if (append) {
            // bug: duplicate tags are added, in the api those are filtered
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
          if (colorclass !== undefined && colorclass !== -1) {
            state.fileIndexItems[index].colorClass = colorclass;
            // add to list of colorclasses that can be selected
            if (state.colorClassUsage && state.colorClassUsage.indexOf(colorclass) === -1) state.colorClassUsage.push(colorclass);

            // checks the list of colorclasses that can be selected and removes the ones without 
            // only usefull when there are no colorclasses selected

            if (state.colorClassActiveList === undefined) state.colorClassActiveList = [];
            if (state.colorClassActiveList.length === 0) {
              state.colorClassUsage.forEach(usage => {
                const even = (element: IFileIndexItem) => element.colorClass === usage;
                // some is not working in context of jest
                if (!state.fileIndexItems.some(even).valueOf()) {
                  var indexer = state.colorClassUsage.indexOf(usage);
                  state.colorClassUsage.splice(indexer, 1);
                }
              });
            }
          }
          state.fileIndexItems[index].lastEdited = new Date().toISOString();
        }
      });

      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "set":
      // ignore the cache
      return action.payload;
    case "force-reset":
      // also update the cache
      return updateCache(action.payload);
    case "add":
      var filterOkCondition = (value: IFileIndexItem) => {
        return (value.status === IExifStatus.Ok || value.status === IExifStatus.Default);
      };

      var concattedFileIndexItems = [...Array.from(action.add), ...state.fileIndexItems];
      concattedFileIndexItems = new ArrayHelper().UniqueResults(concattedFileIndexItems, 'filePath');

      // order by this to match c#
      var fileIndexItems = concattedFileIndexItems.sort((a, b) => a.fileName.localeCompare(b.fileName, 'en', { sensitivity: 'base' }));

      fileIndexItems = fileIndexItems.filter(filterOkCondition);
      return updateCache({ ...state, fileIndexItems, lastUpdated: new Date() });
  }
}

/**
 * Update the cache based on the keys
 */
function updateCache(stateLocal: IArchiveProps): IArchiveProps {
  if (stateLocal.pageType !== PageType.Archive) return stateLocal;
  var urlObject = { f: stateLocal.subPath, colorClass: stateLocal.colorClassActiveList, collections: stateLocal.collections } as IUrl;
  new FileListCache().CacheSetObject(urlObject, { ...stateLocal });
  return stateLocal;
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
export const useArchiveContext = () => React.useContext(ArchiveContext);

/**
 * default values
 */
export const defaultStateFallback = (state: IArchiveProps) => {
  if (!state) state = {
    ...newIArchive(),
    collectionsCount: 0,
    fileIndexItems: [],
    pageType: PageType.Loading,
    isReadOnly: true,
    breadcrumb: [],
    relativeObjects: {} as IRelativeObjects,
    subPath: "/",
    colorClassActiveList: [],
    colorClassUsage: [],
  };
  return state;
}