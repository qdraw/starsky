import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import * as Archive from "../containers/archive";
import * as Login from "../containers/login";
import * as Search from "../containers/search";
import * as Trash from "../containers/trash";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { newIArchive } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import {
	IFileIndexItem,
	newIFileIndexItem
} from "../interfaces/IFileIndexItem";
import ArchiveContextWrapper, {
	ArchiveEventListenerUseEffect
} from "./archive-wrapper";

describe("ArchiveContextWrapper", () => {
	it("renders", () => {
		shallow(<ArchiveContextWrapper {...newIArchive()} />);
	});

	describe("with mount", () => {
		it("check if archive is mounted", () => {
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Archive
			} as IArchiveProps;
			var archive = jest
				.spyOn(Archive, "default")
				.mockImplementationOnce(() => {
					return <></>;
				});

			args.fileIndexItems.push({} as IFileIndexItem);
			mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
			expect(archive).toBeCalled();
		});

		it("check if search is mounted", () => {
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Search,
				searchQuery: ""
			} as IArchiveProps;
			var search = jest.spyOn(Search, "default").mockImplementationOnce(() => {
				return <></>;
			});

			// for loading
			jest.spyOn(Archive, "default").mockImplementationOnce(() => {
				return <></>;
			});

			args.fileIndexItems.push({} as IFileIndexItem);

			mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
			expect(search).toBeCalled();
		});

		it("check if Unauthorized/Login is mounted", () => {
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Unauthorized
			} as IArchiveProps;
			var login = jest.spyOn(Login, "default").mockImplementationOnce(() => {
				return <></>;
			});

			args.fileIndexItems.push({} as IFileIndexItem);
			mount(<ArchiveContextWrapper {...args} />);
			expect(login).toBeCalled();
		});

		it("check if Trash is mounted", () => {
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Trash
			} as IArchiveProps;
			var login = jest.spyOn(Trash, "default").mockImplementationOnce(() => {
				return <></>;
			});

			args.fileIndexItems.push({} as IFileIndexItem);
			mount(<ArchiveContextWrapper {...args} />);
			expect(login).toBeCalled();
		});
	});

	describe("no context", () => {
		it("No context if used", () => {
			jest.spyOn(React, "useContext").mockImplementationOnce(() => {
				return { state: null, dispatch: jest.fn() };
			});
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Search
			} as IArchiveProps;
			var component = mount(
				<ArchiveContextWrapper {...args}></ArchiveContextWrapper>
			);

			expect(component.text()).toBe("(ArchiveWrapper) = no state");
		});

		it("No fileIndexItems", () => {
			jest.spyOn(React, "useContext").mockImplementationOnce(() => {
				return { state: { fileIndexItems: undefined }, dispatch: jest.fn() };
			});
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Search
			} as IArchiveProps;
			var component = mount(
				<ArchiveContextWrapper {...args}></ArchiveContextWrapper>
			);

			expect(component.text()).toBe("");
		});

		it("No pageType", () => {
			jest.spyOn(React, "useContext").mockImplementationOnce(() => {
				return {
					state: { fileIndexItems: [], pageType: undefined },
					dispatch: jest.fn()
				};
			});
			var args = {
				...newIArchive(),
				fileIndexItems: [],
				pageType: PageType.Search
			} as IArchiveProps;
			var component = mount(
				<ArchiveContextWrapper {...args}></ArchiveContextWrapper>
			);

			expect(component.text()).toBe("");
		});
	});

	describe("ArchiveEventListenerUseEffect / updateArchiveFromEvent", () => {
		const { location } = window;
		/**
		 * Mock the location feature
		 * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
		 */
		beforeAll(() => {
			// @ts-ignore
			delete window.location;
			// @ts-ignore
			window.location = {
				search: "/?f=/"
			};
		});

		afterAll((): void => {
			window.location = location;
		});

		it("Check if event is received", (done) => {
			var fileNameJestFn = {
				localeCompare: jest.fn()
			};
			var dispatch = (e: any) => {
				// should ignore the first one
				expect(e).toStrictEqual({
					add: [
						{
							colorclass: undefined,
							description: "",
							fileName: fileNameJestFn,
							filePath: "/test.jpg",
							parentDirectory: "/",
							tags: "",
							title: ""
						}
					],
					type: "add"
				});
				done();
			};

			function TestComponent() {
				ArchiveEventListenerUseEffect(dispatch);
				return <></>;
			}

			var component = mount(<TestComponent />);

			var detail = [
				{
					colorclass: undefined,
					...newIFileIndexItem(),
					filePath: "/test.jpg",
					fileName: fileNameJestFn,
					parentDirectory: "/"
				}
			];
			var event = new CustomEvent(useSocketsEventName, {
				detail
			});

			act(() => {
				document.body.dispatchEvent(event);
			});

			component.unmount();
		});

		it("When outside current directory it should be ignored", (done) => {
			var dispatch = (e: any) => {
				// should ignore the first one
				expect(e).toStrictEqual({
					add: [],
					type: "add"
				});
				done();
			};

			function TestComponent() {
				ArchiveEventListenerUseEffect(dispatch);
				return <></>;
			}

			var component = mount(<TestComponent />);

			var detail = [
				{
					colorclass: undefined,
					...newIFileIndexItem(),
					filePath: "/test.jpg",
					fileName: "test.jpg",
					parentDirectory: "/__something__different"
				}
			];
			var event = new CustomEvent(useSocketsEventName, {
				detail
			});

			act(() => {
				document.body.dispatchEvent(event);
			});

			component.unmount();
		});
	});
});
