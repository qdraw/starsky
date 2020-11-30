import * as types from "./action-types";

export function createCourse(course: any) {
	return { type: types.CREATE_COURSE, course };
}
