import { RouteComponentProps } from "@reach/router";
import React, { FunctionComponent } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import useLocation from "../hooks/use-location";
import useTrashList from "../hooks/use-trashlist";
import { PageType } from "../interfaces/IDetailView";
import { URLPath } from "../shared/url-path";

interface ITrashPageProps {}

const TrashPage: FunctionComponent<RouteComponentProps<ITrashPageProps>> = (
	props
) => {
	var history = useLocation();

	var urlObject = new URLPath().StringToIUrl(history.location.search);
	var searchList = useTrashList(urlObject.p);

	if (!searchList) return <>Something went wrong</>;
	if (!searchList.archive) return <>Something went wrong</>;
	if (searchList.pageType === PageType.Loading)
		return (
			<Preloader
				isTransition={false}
				isOverlay={true}
				isDetailMenu={false}
			></Preloader>
		);

	return <ArchiveContextWrapper {...searchList.archive} />;
};

export default TrashPage;
