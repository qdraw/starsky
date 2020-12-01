import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { bindActionCreators, Dispatch } from "redux";
import Preloader from "../../components/atoms/preloader/preloader";
import ArchiveLayout from "../../components/organisms/archive-layout/archive-layout";
import { loadLibrary } from "../../global-store/actions/library-actions";
import { updateSubPath } from "../../global-store/actions/subpath-actions";
import { FileExtensions } from "../../shared/file-extensions";
import { URLPath } from "../../shared/url-path";

export const MediaLibraryContainer: React.FunctionComponent<IMediaLibraryContainerProps> = ({
	locationSearch,
	library,
	loading,
	...props
}) => {
	const [subPath, setSubPath] = useState("");

	function getSubPath() {
		if (locationSearch === undefined) return "";

		const urlObject = new URLPath().StringToIUrl(locationSearch);
		return !urlObject.f ? "/" : urlObject.f;
	}

	useEffect(() => {
		if (locationSearch === undefined) return;

		setSubPath(getSubPath());

		props.actions?.updateSubPath(getSubPath());
		if (!library) {
			props.actions?.loadLibrary(subPath);
		}
		// run only when url changed
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [locationSearch]);

	return (
		<>
			{loading ? <Preloader isOverlay={true} /> : null}
			{library ? <ArchiveLayout subPath={subPath} archive={library}></ArchiveLayout> : null}
		</>
	);
};

export interface IMediaLibraryContainerProps {
	locationSearch?: string;
	actions?: {
		loadLibrary(f: string): void;
		updateSubPath(f: string): void;
	};
	library?: any;
	loading?: boolean;
	subPath?: string;
	apiCallsInProgress?: number;
}

function mapStateToProps({ apiCallsInProgress, subPath, ...props }: IMediaLibraryContainerProps) {
	var item = props.library[!subPath ? "" : subPath];
	if (item === undefined && !subPath) {
		const parentPath = new FileExtensions().GetParentPath(!subPath ? "" : subPath);
		item = props.library[parentPath];
	}

	return {
		library: props.library[!subPath ? "" : subPath],
		loading: apiCallsInProgress === undefined ? false : apiCallsInProgress > 0,
	};
}

function mapDispatchToProps(dispatch: Dispatch) {
	return {
		actions: {
			loadLibrary: bindActionCreators(loadLibrary, dispatch),
			updateSubPath: bindActionCreators(updateSubPath, dispatch),
		},
	};
}

export default connect(mapStateToProps, mapDispatchToProps)(MediaLibraryContainer);
