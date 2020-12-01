import React from "react";
import { RouteProps } from "react-router-dom";
import MediaLibraryContainer from "../containers/media-library/media-library-container";

function MediaLibraryPage(props: RouteProps) {
	return <MediaLibraryContainer locationSearch={props.location?.search}></MediaLibraryContainer>;
}

export default MediaLibraryPage;
