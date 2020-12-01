import { Action, UPDATE_SUBPATH } from "../actions/action-types";

export default function subPathReducer(state: string = "", action: Action) {
	switch (action.type) {
		case UPDATE_SUBPATH:
			return action.subPath;
		default:
			return state;
	}
}
