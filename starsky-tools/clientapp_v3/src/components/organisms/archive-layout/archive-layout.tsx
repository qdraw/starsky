import React from "react";
import { PageType } from "../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import Breadcrumbs from "../../molecules/breadcrumbs/breadcrumbs";
import ItemListView from "../../molecules/item-list-view/item-list-view";

interface IArchiveLayout {
	archive: IFileIndexItem[];
	subPath: string;
}

const ArchiveLayout: React.FunctionComponent<IArchiveLayout> = (props) => {
	console.log(props);

	return (
		<>
			{/* <MenuArchive /> */}
			<div className={!true ? "archive" : "archive collapsed"}>
				{/* <ArchiveSidebar {...archive} /> */}

				<div className="content">
					<Breadcrumbs
						breadcrumb={props.subPath.split("/").filter((_, i) => {
							return i !== props.subPath.split("/").length - 1;
						})}
						subPath={props.subPath}
					/>
					{/* <ArchivePagination relativeObjects={archive.relativeObjects} /> */}

					{/* <ColorClassFilter
						itemsCount={archive.collectionsCount}
						subPath={archive.subPath}
						colorClassActiveList={archive.colorClassActiveList}
						colorClassUsage={archive.colorClassUsage}
					/> */}
					<ItemListView
						pageType={PageType.Archive}
						colorClassUsage={[0]}
						fileIndexItems={props.archive}
					></ItemListView>
				</div>
			</div>
		</>
	);
};

export default ArchiveLayout;

// {
// 	/* <>
// 	<MenuArchive />
// 	<div className={!sidebar ? "archive" : "archive collapsed"}>
// 		<ArchiveSidebar {...archive} />

// 		<div className="content">
// 			<Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath} />
// 			<ArchivePagination relativeObjects={archive.relativeObjects} />

// 			<ColorClassFilter
// 				itemsCount={archive.collectionsCount}
// 				subPath={archive.subPath}
// 				colorClassActiveList={archive.colorClassActiveList}
// 				colorClassUsage={archive.colorClassUsage}
// 			/>
// 			<ItemListView {...archive}> </ItemListView>
// 		</div>
// 	</div>
// </>; */
// }
