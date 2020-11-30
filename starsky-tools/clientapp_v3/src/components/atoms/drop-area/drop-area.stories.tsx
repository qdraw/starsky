import { storiesOf } from "@storybook/react";
import React from "react";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ModalDropAreaFilesAdded from "../modal-drop-area-files-added/modal-drop-area-files-added";
import DropArea from "./drop-area";

storiesOf("components/atoms/drop-area", module).add("default", () => {
	const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(
		newIFileIndexItemArray()
	);

	return (
		<>
			<div>
				Drag 'n drop file to show
				<br />
				<br />
			</div>
			<DropArea
				enableDragAndDrop={true}
				enableInputButton={true}
				callback={(add) => {
					setDropAreaUploadFilesList(add);
				}}
				endpoint="/starsky/api/import"
			/>

			{/* Upload drop Area */}
			{dropAreaUploadFilesList.length !== 0 ? (
				<ModalDropAreaFilesAdded
					handleExit={() => setDropAreaUploadFilesList(newIFileIndexItemArray())}
					uploadFilesList={dropAreaUploadFilesList}
					isOpen={dropAreaUploadFilesList.length !== 0}
				/>
			) : null}
		</>
	);
});
