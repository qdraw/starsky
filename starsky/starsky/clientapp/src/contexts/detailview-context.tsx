import React, { createContext, useMemo } from "react";
import {
  IDetailView,
  IRelativeObjects,
  PageType
} from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { Orientation, newIFileIndexItem } from "../interfaces/IFileIndexItem";
import { IUrl } from "../interfaces/IUrl";
import { FileListCache } from "../shared/filelist-cache";

const DetailViewContext = createContext<IDetailViewContext>(
  {} as IDetailViewContext
);

type ReactNodeProps = { children: React.ReactNode };

const initialState: IDetailView = {
  breadcrumb: [],
  fileIndexItem: newIFileIndexItem(),
  relativeObjects: {} as IRelativeObjects,
  subPath: "/",
  pageType: PageType.DetailView,
  colorClassActiveList: [],
  isReadOnly: false,
  dateCache: Date.now()
};

export type DetailViewAction =
  | {
      type: "append";
      tags?: string;
    }
  | {
      type: "remove";
      tags?: string;
    }
  | {
      type: "update";
      filePath: string;
      tags?: string;
      colorclass?: number;
      description?: string;
      title?: string;
      fileHash?: string;
      orientation?: Orientation;
      status?: IExifStatus;
      lastEdited?: string;
      dateTime?: string;
      latitude?: number;
      longitude?: number;
      locationCity?: string;
      locationCountry?: string;
      locationCountryCode?: string;
      locationState?: string;
    }
  | {
      type: "reset";
      payload: IDetailView;
    };

export type IDetailViewContext = {
  state: IDetailView;
  dispatch: React.Dispatch<DetailViewAction>;
};
export function detailviewReducer(
  state: IDetailView,
  action: DetailViewAction
): IDetailView {
  switch (action.type) {
    case "remove":
      if (action.tags && state.fileIndexItem.tags !== undefined)
        state.fileIndexItem.tags = state.fileIndexItem.tags.replace(
          action.tags,
          ""
        );
      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "append":
      if (action.tags) state.fileIndexItem.tags += "," + action.tags;
      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "update":
       
      const {
        filePath,
        tags,
        description,
        title,
        status,
        colorclass,
        fileHash,
        orientation,
        lastEdited,
        dateTime,
        latitude,
        longitude,
        locationCity,
        locationCountry,
        locationCountryCode,
        locationState
      } = action;

      if (filePath !== state.fileIndexItem.filePath) {
        console.log(
          `Error: filePath is not the same ${filePath} != ${state.fileIndexItem.filePath}`
        );
        return state;
      }

      if (tags !== undefined) state.fileIndexItem.tags = tags;
      if (description !== undefined)
        state.fileIndexItem.description = description;
      if (title !== undefined) state.fileIndexItem.title = title;
      if (colorclass !== undefined && colorclass !== -1)
        state.fileIndexItem.colorClass = colorclass;
      if (status) state.fileIndexItem.status = status;
      if (fileHash) state.fileIndexItem.fileHash = fileHash;
      if (orientation) state.fileIndexItem.orientation = orientation;
      if (lastEdited) state.fileIndexItem.lastEdited = lastEdited;
      if (dateTime) state.fileIndexItem.dateTime = dateTime;
      if (latitude) state.fileIndexItem.latitude = latitude;
      if (longitude) state.fileIndexItem.longitude = longitude;
      if (locationCity) state.fileIndexItem.locationCity = locationCity;
      if (locationCountry) {
        state.fileIndexItem.locationCountry = locationCountry;
      }
      if (locationCountryCode) {
        state.fileIndexItem.locationCountryCode = locationCountryCode;
      }
      if (locationState) state.fileIndexItem.locationState = locationState;

      // Need to update otherwise other events are not triggerd
      return updateCache({ ...state, lastUpdated: new Date() });
    case "reset":
      // this is triggert a lot when loading a page
      return action.payload;
  }
}

/**
 * Update the cache based on the keys
 */
function updateCache(stateLocal: IDetailView): IDetailView {
  if (!stateLocal.fileIndexItem) {
    return stateLocal;
  }
  const urlObject = {
    f: stateLocal.subPath,
    colorClass: stateLocal.colorClassActiveList,
    collections: stateLocal.collections
  } as IUrl;
  new FileListCache().CacheSetObject(urlObject, { ...stateLocal });
  return stateLocal;
}

function DetailViewContextProvider({ children }: ReactNodeProps) {
  // [A]
  const [state, dispatch] = React.useReducer(detailviewReducer, initialState);
  // Use useMemo to memoize the value object
  const value1 = useMemo(() => ({ state, dispatch }), [state, dispatch]);

  // [B]
  return (
    <DetailViewContext.Provider value={value1}>
      {children}
    </DetailViewContext.Provider>
  );
}

const DetailViewContextConsumer = DetailViewContext.Consumer;

export {
  DetailViewContext,
  DetailViewContextConsumer,
  DetailViewContextProvider
};

export const useDetailViewContext = () => React.useContext(DetailViewContext);
