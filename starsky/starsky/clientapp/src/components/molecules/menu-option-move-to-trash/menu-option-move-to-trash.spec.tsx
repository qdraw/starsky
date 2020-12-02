import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import {
	IConnectionDefault,
	newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import {
	IFileIndexItem,
	newIFileIndexItem,
	newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch-post";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

describe("MenuOptionMoveToTrash", () => {
	it("renders", () => {
		var test = {
			...newIArchive(),
			fileIndexItems: newIFileIndexItemArray()
		} as IArchiveProps;
		shallow(
			<MenuOptionMoveToTrash
				setSelect={jest.fn()}
				select={["test.jpg"]}
				isReadOnly={true}
				state={test}
				dispatch={jest.fn()}
			/>
		);
	});

	describe("context", () => {
		it("check if dispatch", async () => {
			jest.spyOn(FetchPost, "default").mockReset();
			var test = {
				...newIArchive(),
				fileIndexItems: [
					{
						...newIFileIndexItem(),
						parentDirectory: "/",
						fileName: "test.jpg"
					} as IFileIndexItem
				]
			} as IArchiveProps;

			const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
				{
					...newIConnectionDefault(),
					data: null,
					statusCode: 200
				}
			);
			var fetchPostSpy = jest
				.spyOn(FetchPost, "default")
				.mockImplementationOnce(() => mockIConnectionDefault);

			var dispatch = jest.fn();
			var component = await mount(
				<MenuOptionMoveToTrash
					setSelect={jest.fn()}
					select={["test.jpg"]}
					isReadOnly={false}
					state={test}
					dispatch={dispatch}
				>
					t
				</MenuOptionMoveToTrash>
			);

			await act(async () => {
				await component.find("li").simulate("click");
			});

			expect(fetchPostSpy).toBeCalled();
			expect(dispatch).toBeCalled();
			expect(dispatch).toBeCalledWith({
				toRemoveFileList: ["/test.jpg"],
				type: "remove"
			});
		});
	});
});
