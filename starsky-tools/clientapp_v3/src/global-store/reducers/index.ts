import { combineReducers } from "redux";
// apiCallsInProgress
import apiCallsInProgress from "./api-status-reducer";
// libraryReducer
import library from "./library-reducer";
// subPathReducer
import subPath from "./sub-path-reducer";

const rootReducer = combineReducers({
	library,
	subPath,
	apiCallsInProgress,
});

export default rootReducer;
