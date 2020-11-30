import { combineReducers } from "redux";
import courses from "./course-reducer";

const rootReducer = combineReducers({
  courses
});

export default rootReducer;
