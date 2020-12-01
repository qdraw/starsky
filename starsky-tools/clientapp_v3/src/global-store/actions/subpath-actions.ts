import * as types from "./action-types";

export function updateSubPath(subPath: string) {
	return { type: types.UPDATE_SUBPATH, subPath };
}
