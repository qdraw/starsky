import { applyMiddleware, compose, createStore } from "redux";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import thunk from "redux-thunk";
import rootReducer from "../reducers";

export default function configureStore(initialState = {}) {
	if (!process.env.NODE_ENV || process.env.NODE_ENV === "development") {
		// add support for Redux dev tools
		const composeEnhancers = (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

		return createStore(
			rootReducer,
			initialState,
			composeEnhancers(applyMiddleware(thunk, reduxImmutableStateInvariant()))
		);
	}
	// Production
	return createStore(rootReducer, initialState, applyMiddleware(thunk));
}
