import { Action } from "redux";
import { API_CALL_ERROR, BEGIN_API_CALL } from "../actions/action-types";
import initialState from "./initial-state";

function actionTypeEndsInSuccess(type: string) {
	return type.substring(type.length - 8) === "_SUCCESS";
}

export default function apiCallStatusReducer(
	state = initialState.apiCallsInProgress,
	action: Action
) {
	if (action.type === BEGIN_API_CALL) {
		return state + 1;
	} else if (action.type === API_CALL_ERROR || actionTypeEndsInSuccess(action.type)) {
		return state - 1;
	}

	return state;
}
