import React from "react";
import { Route, Switch } from "react-router-dom";
import MediaLibraryPage from "../pages/media-library-page";
import NotFoundPage from "../pages/not-found-page";

function RouterApp() {
	return (
		<div className="container-fluid">
			<Switch>
				<Route exact path="/" component={MediaLibraryPage} />
				<Route component={NotFoundPage} />
			</Switch>
		</div>
	);
}

export default RouterApp;
