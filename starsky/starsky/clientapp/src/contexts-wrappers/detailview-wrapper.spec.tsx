import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import * as DetailView from "../containers/detailview";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { mountReactHook } from "../hooks/___tests___/test-hook";
import { IDetailView, newDetailView } from "../interfaces/IDetailView";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import DetailViewWrapper, {
	DetailViewEventListenerUseEffect
} from "./detailview-wrapper";

describe("DetailViewWrapper", () => {
	it("renders", () => {
		shallow(<DetailViewWrapper {...newDetailView()} />);
	});

	describe("with mount", () => {
		it("check if DetailView is mounted", () => {
			var args = { ...newDetailView() } as IDetailView;
			var detailView = jest
				.spyOn(DetailView, "default")
				.mockImplementationOnce(() => {
					return <></>;
				});

			const compontent = mount(<DetailViewWrapper {...args} />);
			expect(detailView).toBeCalled();
			compontent.unmount();
		});

		it("check if dispatch is called", () => {
			var contextValues = { state: newIFileIndexItem(), dispatch: jest.fn() };
			jest.spyOn(React, "useContext").mockImplementationOnce(() => {
				return contextValues;
			});

			var args = {
				...newDetailView(),
				fileIndexItem: newIFileIndexItem()
			} as IDetailView;
			var detailView = jest
				.spyOn(DetailView, "default")
				.mockImplementationOnce(() => {
					return <></>;
				});

			const compontent = mount(<DetailViewWrapper {...args} />);

			expect(contextValues.dispatch).toBeCalled();
			expect(detailView).toBeCalled();
			compontent.unmount();
		});
	});

	describe("no context", () => {
		it("No context if used", () => {
			jest.spyOn(React, "useContext").mockImplementationOnce(() => {
				return { state: null, dispatch: jest.fn() };
			});
			var args = { ...newDetailView() } as IDetailView;
			var compontent = mount(<DetailViewWrapper {...args} />);

			expect(compontent.text()).toBe("(DetailViewWrapper) = no state");
			compontent.unmount();
		});
	});

	describe("DetailViewEventListenerUseEffect", () => {
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
				search: "/?f=/test.jpg"
			};
		});

		afterAll((): void => {
			window.location = location;
		});

		it("Check if event is received", () => {
			// var dispatch = (e: any) => {
			// 	console.log("-dfsdfnlk");

			// 	// should ignore the first one
			// 	expect(e).toStrictEqual(detail[1]);
			// };

			var dispatch = jest.fn();
			document.body.innerHTML = "";
			var result = mountReactHook(DetailViewEventListenerUseEffect, [dispatch]);

			var detail = [
				{
					...newIFileIndexItem()
					// should ignore this one
				},
				{
					colorclass: undefined,
					...newIFileIndexItem(),
					filePath: "/test.jpg",
					type: "update"
				}
			];
			var event = new CustomEvent(useSocketsEventName, {
				detail
			});

			document.body.dispatchEvent(event);

			expect(dispatch).toBeCalled();
			expect(dispatch).toBeCalledWith(detail[1]);

			var element = (result.componentMount as any) as ReactWrapper;
			element.unmount();
		});
	});
});
