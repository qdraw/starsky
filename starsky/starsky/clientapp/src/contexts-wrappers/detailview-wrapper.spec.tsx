import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import * as DetailView from "../containers/detailview";
import * as useDetailViewContext from "../contexts/detailview-context";
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
			var contextValues = {
				state: { fileIndexItem: newIFileIndexItem() },
				dispatch: jest.fn()
			} as any;

			jest
				.spyOn(useDetailViewContext, "useDetailViewContext")
				.mockImplementationOnce(() => contextValues as any)
				.mockImplementationOnce(() => contextValues as any)
				.mockImplementationOnce(() => contextValues as any);

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
			var contextValues = {
				state: null,
				dispatch: jest.fn()
			} as any;

			jest
				.spyOn(useDetailViewContext, "useDetailViewContext")
				.mockImplementationOnce(() => contextValues as any);

			var args = { ...newDetailView() } as IDetailView;
			var compontent = mount(<DetailViewWrapper {...args} />);

			expect(compontent.text()).toBe("");
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
