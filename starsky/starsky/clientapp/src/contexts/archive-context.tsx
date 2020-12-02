import * as React from "react";
import { newIArchive } from "../interfaces/IArchive";
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
			type: "set";
			payload: IArchiveProps;
	  }
	| {
			type: "force-reset";
			payload: IArchiveProps;
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
		case "remove":
			// files == subpath style not only the name (/dir/file.jpg)
			var { toRemoveFileList } = action;

			var deletedFilesCount = 0;
			let afterFileIndexItems: IFileIndexItem[] = [];

			state.fileIndexItems.forEach((item) => {
				if (toRemoveFileList.indexOf(item.filePath) === -1) {
					afterFileIndexItems.push(item);
				} else {
					deletedFilesCount++;
				}
			});

			// to update the total results
			var collectionsCount = state.collectionsCount - deletedFilesCount;

			return updateCache({
				...state,
				fileIndexItems: afterFileIndexItems,
				collectionsCount,
				lastUpdated: new Date()
			});
		case "update":
			var {
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
						if (tags) state.fileIndexItems[index].tags = tags;
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

			// Need to update otherwise other events are not triggerd
			return updateCache({ ...state, lastUpdated: new Date() });
		case "set":
			// ignore the cache
			if (!action.payload.fileIndexItems) return action.payload;
			return {
				...action.payload,
				fileIndexItems: new ArrayHelper().UniqueResults(
					action.payload.fileIndexItems,
					"filePath"
				)
			};
		case "force-reset":
			// also update the cache
			return updateCache({
				...action.payload,
				fileIndexItems: new ArrayHelper().UniqueResults(
					action.payload.fileIndexItems,
					"filePath"
				)
			});
		case "add":
			var filterOkCondition = (value: IFileIndexItem) => {
				return (
					value.status === IExifStatus.Ok ||
					value.status === IExifStatus.Default
				);
			};
			var concatenatedFileIndexItems = [
				...Array.from(action.add),
				...state.fileIndexItems
			];

			var toSortOnParm = state.collections ? "fileCollectionName" : "filePath";
			concatenatedFileIndexItems = new ArrayHelper().UniqueResults(
				concatenatedFileIndexItems,
				toSortOnParm
			);

			// order by this to match c# AND not supported in jest
			try {
				var fileIndexItems = [...concatenatedFileIndexItems].sort((a, b) =>
					a.fileName.localeCompare(b.fileName, "en", { sensitivity: "base" })
				);
			} catch (error) {
				fileIndexItems = concatenatedFileIndexItems;
			}

			fileIndexItems = fileIndexItems.filter(filterOkCondition);
			state = { ...state, fileIndexItems, lastUpdated: new Date() };
			UpdateColorClassUsageActiveListLoop(state);
			return updateCache(state);
	}
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
	if (state.colorClassUsage.indexOf(colorclass) === -1)
		state.colorClassUsage.push(colorclass);

	if (state.colorClassActiveList === undefined) state.colorClassActiveList = [];
	// when the user selects by colorclass
	if (state.colorClassActiveList.length >= 1) return;

	// checks the list of colorclasses that can be selected and removes the ones without
	// only usefull when there are no colorclasses selected

	state.colorClassUsage.forEach((usage) => {
		const even = (element: IFileIndexItem) => element.colorClass === usage;
		// some is not working in context of jest
		if (!state.fileIndexItems.some(even).valueOf()) {
			var indexer = state.colorClassUsage.indexOf(usage);
			state.colorClassUsage.splice(indexer, 1);
		}
	});
	return;
}

/**
 * Update the cache based on the keys
 */
function updateCache(stateLocal: IArchiveProps): IArchiveProps {
	if (stateLocal.pageType !== PageType.Archive) return stateLocal;
	var urlObject = {
		f: stateLocal.subPath,
		colorClass: stateLocal.colorClassActiveList,
		collections: stateLocal.collections
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
