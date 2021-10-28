import { render, RenderResult } from "@testing-library/react";
import React from "react";
import Modal from "./modal";

describe("Modal", () => {
  it("renders", () => {
    window.scrollTo = jest.fn();
    render(
      <Modal id="test2-modal" isOpen={true} handleExit={() => {}}>
        &nbsp;
      </Modal>
    );
  });

  describe("Close Modal", () => {
    function renderModal(): any {
      window.scrollTo = jest.fn();
      var handleExit = jest.fn();
      const element = render(
        <Modal id="test-modal" isOpen={true} handleExit={handleExit}>
          &nbsp;
        </Modal>
      );
      return {
        handleExit,
        element
      };
    }

    it("modal-exit-button", () => {
      const { handleExit, element } = renderModal();

      element.queryAllByTestId("modal-exit-button")[0].click();
      expect(handleExit).toBeCalled();
      element.unmount();
    });

    it("modal-bg", () => {
      const { handleExit, element } = renderModal();

      element.queryAllByTestId("modal-bg")[0].click();
      expect(handleExit).toBeCalled();
      element.unmount();
    });
  });

  describe("Open Modal", () => {
    function renderModal2(): [
      jest.Mock<any, any>,
      RenderResult<
        typeof import("/data/git/starsky/starsky/starsky/clientapp/node_modules/@testing-library/dom/types/queries"),
        HTMLElement
      >
    ] {
      const spyScrollTo = jest.fn();
      window.scrollTo = spyScrollTo;

      var handleExit = jest.fn() as any;
      const component = render(
        <div>
          <Modal id="test-modal" isOpen={true} handleExit={handleExit}>
            &nbsp;
          </Modal>
          <div className="root" />
        </div>
      );
      return [spyScrollTo, component];
    }

    it("sould open modal", () => {
      const [spyScrollTo, component] = renderModal2();

      var element = document.body.querySelector(".modal-bg--open");

      expect(element).toBeTruthy();

      component.unmount();

      spyScrollTo.mockClear();
    });
  });
});
