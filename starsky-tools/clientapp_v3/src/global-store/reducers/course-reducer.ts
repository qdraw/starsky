import * as types from "../actions/action-types";

export default function courseReducer(state = [], action: any) {
	switch (action.type) {
		case types.CREATE_COURSE:
			return [...state, { ...action.course }];
		default:
			return state;
	}
}
