import * as React from "react";
import { newIArchive, SortType } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import {
  IRelativeObjects,
  newIRelativeObjects,
  PageType
} from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { IFileIndexItem } from "../interfaces/IFileIndexItem";
import { IUrl } from "../interfaces/IUrl";
import ArrayHelper from "../shared/array-helper";
import { FileListCache } from "../shared/filelist-cache";
import { sorter } from "./sorter";

const ArchiveContext = React.createContext<IArchiveContext>(
  {} as IArchiveContext
);

export type IArchiveContext = {
  state: State;
  dispatch: React.Dispatch<ArchiveAction>;
};

type ReactNodeProps = { children: React.ReactNode };
export type ArchiveAction =
  | {
      type: "remove";
      toRemoveFileList: string[];
    }
  | {
      type: "rename-folder";
      path: string;
    }
  | {
      type: "update";
      tags?: string;
      colorclass?: number;
      description?: string;
      title?: string;
      append?: boolean;
      select: string[];
      fileHash?: string;
    }
  | {
      // ignores cache
      type: "set";
      payload: IArchiveProps;
    }
  | {
      // also update the cache
      type: "force-reset";
      payload: IArchiveProps;
    }
  | {
      type: "remove-folder";
    }
  | {
      type: "add";
      add: Array<IFileIndexItem>;
    };

type State = IArchiveProps;

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

export function archiveReducer(state: State, action: ArchiveAction): State {
  switch (action.type) {
    case "remove-folder":
      return updateCache({
        ...state,
        colorClassUsage: [],
        colorClassActiveList: [],
        collectionsCount: 0,
        fileIndexItems: []
      });
    case "remove":
      // files == subpath style not only the name (/dir/file.jpg)
      const { toRemoveFileList } = action;

      let deletedFilesCount = 0;
      const afterFileIndexItems: IFileIndexItem[] = [];

      state.fileIndexItems.forEach((item) => {
        if (toRemoveFileList.indexOf(item.filePath) === -1) {
          afterFileIndexItems.push(item);
        } else {
          deletedFilesCount++;
        }
      });

      // to update the total results
      const collectionsCount = state.collectionsCount - deletedFilesCount;

      const newState = {
        ...state,
        fileIndexItems: afterFileIndexItems,
        collectionsCount,
        lastUpdated: new Date()
      };

      // when you remove the last item of the directory
      if (newState.fileIndexItems.length === 0) {
        newState.colorClassUsage = [];
      }
      return updateCache(newState);
    case "update":
      const {
        select,
        tags,
        description,
        title,
        append,
        colorclass,
        fileHash
      } = action;

      state.fileIndexItems.forEach((item, index) => {
        if (select.indexOf(item.fileName) !== -1) {
          if (append) {
            // bug: duplicate tags are added, in the api those are filtered
            if (tags) state.fileIndexItems[index].tags += ", " + tags;
            if (description)
              state.fileIndexItems[index].description += description;
            if (title) state.fileIndexItems[index].title += title;
          } else {
            if (tags !== undefined) state.fileIndexItems[index].tags = tags;
            if (description)
              state.fileIndexItems[index].description = description;
            if (title) state.fileIndexItems[index].title = title;
          }
          if (fileHash) state.fileIndexItems[index].fileHash = fileHash;
          // colorclass = 0 ==> colorless/no-color
          if (colorclass !== undefined && colorclass !== -1) {
            state.fileIndexItems[index].colorClass = colorclass;
            UpdateColorClassUsageActiveList(state, colorclass);
          }
          state.fileIndexItems[index].lastEdited = new Date().toISOString();
        }
      });

      // Need to update otherwise other events are not triggered
      return updateCache({ ...state, lastUpdated: new Date() });
    case "set":
      // ignore the cache
      if (!action.payload.fileIndexItems) return action.payload;
      let items = new ArrayHelper().UniqueResults(
        action.payload.fileIndexItems,
        "filePath"
      );

      if (
        action.payload.pageType === PageType.Archive &&
        action.payload.sort &&
        action.payload.sort !== SortType.fileName
      ) {
        items = sorter(items, action.payload.sort);
      }
      return {
        ...action.payload,
        fileIndexItems: items
      };
    case "force-reset":
      // also update the cache
      const forceResetUpdated = {
        ...action.payload,
        fileIndexItems: sorter(
          new ArrayHelper().UniqueResults(
            action.payload.fileIndexItems,
            "filePath"
          )
        )
      };
      return updateCache(forceResetUpdated);
    case "rename-folder":
      return updateCache({ ...state, subPath: action.path });
    case "add":
      if (!action.add) return state;
      const filterOkCondition = (value: IFileIndexItem) => {
        return (
          value.status === IExifStatus.Ok ||
          value.status === IExifStatus.Default ||
          value.status === IExifStatus.OperationNotSupported // pushed when trying to create a map that already exist
        );
      };

      const actionAdd = filterColorClassBeforeAdding(state, action.add);

      // when adding items outside current colorclass filter
      if (actionAdd.length === 0) {
        new FileListCache().CacheCleanEverything();
        return state;
      }

      let concatenatedFileIndexItems = [
        ...Array.from(actionAdd).filter(filterOkCondition),
        ...state.fileIndexItems
      ];

      const toSortOnParm = state.collections
        ? "fileCollectionName"
        : "filePath";

      // only the order within fileCollectionName, not the actual order of the list
      concatenatedFileIndexItems = CollectionsSortOnImageFormat(
        concatenatedFileIndexItems,
        state.collections
      );

      concatenatedFileIndexItems = new ArrayHelper().UniqueResults(
        concatenatedFileIndexItems,
        toSortOnParm
      );

      let fileIndexItems = sorter(concatenatedFileIndexItems, state.sort);

      // remove deleted items
      for (const deleteItem of Array.from(actionAdd).filter(
        (value) =>
          value.status === IExifStatus.Deleted ||
          value.status === IExifStatus.NotFoundNotInIndex ||
          value.status === IExifStatus.NotFoundSourceMissing
      )) {
        const index = fileIndexItems.findIndex(
          (x) => x.filePath === deleteItem.filePath
        );
        if (index !== -1) {
          fileIndexItems.splice(index, 1);
        }
      }

      state = { ...state, fileIndexItems, lastUpdated: new Date() };
      // when you remove the last item of the directory
      if (state.fileIndexItems.length === 0) {
        state.colorClassUsage = [];
      }
      UpdateColorClassUsageActiveListLoop(state);
      return updateCache(state);
  }
}
/**
 * filter colorclass input
 * @param state contains current filter active
 * @param actionAdd item to add
 */
function filterColorClassBeforeAdding(
  state: IArchiveProps,
  actionAdd: IFileIndexItem[]
) {
  if (!state.colorClassActiveList || state.colorClassActiveList.length === 0) {
    return actionAdd;
  }

  actionAdd = actionAdd.filter((value: IFileIndexItem) => {
    return (
      value.colorClass &&
      state.colorClassActiveList.indexOf(value.colorClass) >= 1
    );
  });
  return actionAdd;
}

/**
 * Loop of ColorClass Usage is the list of Colorclasses a user can select.
 * @see: UpdateColorClassUsageActiveList
 * @param state - current state
 */
function UpdateColorClassUsageActiveListLoop(state: IArchiveProps) {
  for (let index = 0; index < state.fileIndexItems.length; index++) {
    const colorClass = state.fileIndexItems[index].colorClass;
    if (colorClass === undefined) continue;
    UpdateColorClassUsageActiveList(state, colorClass);
  }
}

/**
 * When sending a list with items make sure that the jpegs are listed first
 * only the order within fileCollectionName, not the actual order of the list
 * @param concatenatedFileIndexItems - the list
 * @param collections - only if collections is on
 * @returns new orderd list
 */
function CollectionsSortOnImageFormat(
  concatenatedFileIndexItems: IFileIndexItem[],
  collections?: boolean
): IFileIndexItem[] {
  if (!collections) return concatenatedFileIndexItems;

  concatenatedFileIndexItems.forEach((item) => {
    const collectionsItems = concatenatedFileIndexItems.filter(
      (x) =>
        x.fileCollectionName === item.fileCollectionName &&
        x.parentDirectory === item.parentDirectory
    );
    const sortedCollectionsItems = sorter(
      collectionsItems,
      SortType.imageFormat
    );
    concatenatedFileIndexItems = concatenatedFileIndexItems.filter(
      (p) => !sortedCollectionsItems.includes(p)
    );
    sortedCollectionsItems.forEach((element) => {
      concatenatedFileIndexItems.push(element);
    });
  });
  return concatenatedFileIndexItems;
}

/**
 * ColorClass Usage is the list of Colorclasses a user can select.
 * This need to be updated based on the colorclasses that are in the list
 * @param state - current state
 * @param colorclass - colorclass that has be added
 */
function UpdateColorClassUsageActiveList(
  state: IArchiveProps,
  colorclass: number
): void {
  if (state.colorClassUsage === undefined) state.colorClassUsage = [];

  // add to list of colorclasses that can be selected
  if (state.colorClassUsage.indexOf(colorclass) === -1) {
    state.colorClassUsage.push(colorclass);
  }

  if (state.colorClassActiveList === undefined) state.colorClassActiveList = [];
  // when the user selects by colorclass
  if (state.colorClassActiveList.length >= 1) return;

  // checks the list of colorclasses that can be selected and removes the ones without
  // only usefull when there are no colorclasses selected

  state.colorClassUsage.forEach((usage) => {
    const existLambda = (element: IFileIndexItem) =>
      element.colorClass === usage;
    // some is not working in context of jest
    if (!state.fileIndexItems.some(existLambda).valueOf()) {
      const indexer = state.colorClassUsage.indexOf(usage);
      state.colorClassUsage.splice(indexer, 1);
    }
  });
}

/**
 * Update the cache based on the keys
 */
function updateCache(stateLocal: IArchiveProps): IArchiveProps {
  if (stateLocal.pageType !== PageType.Archive) return stateLocal;
  const urlObject = {
    f: stateLocal.subPath,
    colorClass: stateLocal.colorClassActiveList,
    collections: stateLocal.collections,
    sort: stateLocal.sort
  } as IUrl;
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
  if (!state)
    state = {
      ...newIArchive(),
      collectionsCount: 0,
      fileIndexItems: [],
      pageType: PageType.Loading,
      isReadOnly: true,
      breadcrumb: [],
      relativeObjects: {} as IRelativeObjects,
      subPath: "/",
      colorClassActiveList: [],
      colorClassUsage: []
    };
  return state;
};
