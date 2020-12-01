import { ThunkDispatch } from "redux-thunk";
import { PageType } from "../../interfaces/IDetailView";
import { IExifStatus } from "../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../interfaces/IFileIndexItem";
import FetchGet from "../../shared/fetch-get";
import { FileExtensions } from "../../shared/file-extensions";
import { UrlQuery } from "../../shared/url-query";
import * as types from "./action-types";
import { apiCallError, beginApiCall } from "./api-status-actions";

// Action creator

export function loadCourseSuccess(data: IFileIndexItem[], f: string, status: IExifStatus) {
	return {
		type: types.LOAD_LIBRARY_SUCCESS,
		library: {
			[f]: data,
		},
		status: {
			[f]: status,
		},
	};
}

// export function updateCourseSuccess(course: any) {
// 	return { type: types.UPDATE_COURSE_SUCCESS, course };
// }

// export function deleteCourseOptimistic(course: any) {
// 	return { type: types.DELETE_COURSE_OPTIMISTIC, course };
// }

export function loadLibrary(f: string): Function {
	return async function (dispatch: ThunkDispatch<{}, {}, any>) {
		console.log();

		dispatch(beginApiCall());

		const filePathResult = await FetchGet(new UrlQuery().UrlIndexServerApiPath(f));
		if (filePathResult.statusCode !== 200) {
			dispatch(apiCallError());
		}

		console.log(filePathResult);

		if (filePathResult.data.pageType === PageType.Archive) {
			const event = loadCourseSuccess(
				filePathResult.data.fileIndexItems,
				f,
				filePathResult.data.pageType
			);
			console.log(event);

			dispatch(event);
			return;
		}

		const parentPath = new FileExtensions().GetParentPath(f);

		const parentResult = await FetchGet(new UrlQuery().UrlIndexServerApiPath(parentPath));
		if (parentResult.statusCode !== 200) {
			dispatch(apiCallError());
		}
		const event = loadCourseSuccess(
			[parentResult.data.fileIndexItem],
			parentPath,
			parentResult.data.pageType
		);
		dispatch(event);

		// const result = await FetchGet(new UrlQuery().UrlIndexServerApiPath(f));
		// if (result.statusCode === 200) {
		// 	if (result.data.pageType === PageType.Archive) {
		// 		const event = loadCourseSuccess(result.data.fileIndexItems, f, result.data.pageType);
		// 		dispatch(event);
		// 	} else {
		// 		const parentPath = new FileExtensions().GetParentPath(f);
		// 		const event = loadCourseSuccess(
		// 			[result.data.fileIndexItem],
		// 			parentPath,
		// 			result.data.pageType
		// 		);
		// 		dispatch(event);
		// 	}
		// } else {
		// 	dispatch(apiCallError());
		// }
	};
}
