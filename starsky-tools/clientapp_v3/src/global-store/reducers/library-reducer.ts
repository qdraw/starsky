import { Action, LOAD_LIBRARY_SUCCESS } from "../actions/action-types";
import initalState from "./initial-state";

export default function libraryReducer(state = initalState.library, action: Action) {
	switch (action.type) {
		case LOAD_LIBRARY_SUCCESS:
			console.log(action);

			// console.log({ state, ...action.library });

			return { state };

		// if (state.library === undefined) {
		// }
		// let newState = { ...state };
		// for (const item in action.library) {
		// 	if (state.library[item] === undefined) {
		// 		newState.library[item] = item;
		// 		continue;
		// 	}
		// 	newState.library[item].push(action.library[0]);
		// }
		// console.log(newState);

		// return newState;
		default:
			return state;
	}
}

// import * as types from "../actions/actionTypes";
// import initialState from "./initialState";

// export default function courseReducer(state = initialState.courses, action) {
// 	switch (action.type) {
// 		case types.CREATE_COURSE_SUCCESS:
// 			return [...state, { ...action.course }];
// 		case types.UPDATE_COURSE_SUCCESS:
// 			return state.map((course) => (course.id === action.course.id ? action.course : course));
// 		case types.LOAD_COURSES_SUCCESS:
// 			return action.courses;
// 		case types.DELETE_COURSE_OPTIMISTIC:
// 			return state.filter((course) => course.id !== action.course.id);
// 		default:
// 			return state;
// 	}
// }
