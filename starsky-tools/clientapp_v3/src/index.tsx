import React from "react";
import ReactDOM from "react-dom";
import { Provider as ReduxProvider } from "react-redux";
import { BrowserRouter as Router } from "react-router-dom";
import configureStore from "./global-store/store-config/configure-store";
import "./index.css";
import reportWebVitals from "./reportWebVitals";
import RouterApp from "./router-app/router-app";

const store = configureStore();

ReactDOM.render(
	<React.StrictMode>
		<ReduxProvider store={store}>
			<Router>
				<RouterApp />
			</Router>
		</ReduxProvider>
	</React.StrictMode>,
	document.getElementById("root")
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
