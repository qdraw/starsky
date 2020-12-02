import { mount, shallow } from "enzyme";
import React from "react";
import Modal from "./modal";

describe("Modal", () => {
	it("renders", () => {
		shallow(
			<Modal id="test2-modal" isOpen={true} handleExit={() => {}}></Modal>
		);
	});

	describe("Close Modal", () => {
		var handleExit = jest.fn();
		var element = mount(
			<Modal id="test-modal" isOpen={true} handleExit={handleExit}></Modal>
		);

		it("modal-exit-button", () => {
			element.find(".modal-exit-button").simulate("click");
			expect(handleExit).toBeCalled();
		});

		it("modal-bg", () => {
			element.find(".modal-bg").simulate("click");
			expect(handleExit).toBeCalled();
		});
	});

	describe("Open Modal", () => {
		const spyScrollTo = jest.fn();
		Object.defineProperty(window, "scrollTo", { value: spyScrollTo });

		var handleExit = jest.fn();
		mount(
			<div>
				<Modal id="test-modal" isOpen={false} handleExit={handleExit}></Modal>
				<div className="root" />
			</div>
		);

		it("sould open modal", () => {
			var element = document.body.querySelector(".modal-bg--open");
			expect(element).toBeTruthy();

			// compontent.unmount();
			spyScrollTo.mockClear();
		});
	});
});
