import { fireEvent, render, RenderResult, screen } from "@testing-library/react";
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
      const handleExit = jest.fn();
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

    it("modal-exit-button click", () => {
      const { handleExit, element } = renderModal();

      screen.queryAllByTestId("modal-exit-button")[0].click();
      expect(handleExit).toHaveBeenCalled();
      element.unmount();
    });

    it("modal-exit-button keyDown tab ignores", () => {
      const { handleExit, element } = renderModal();

      const menuOption = screen.queryAllByTestId("modal-exit-button")[0];

      fireEvent.keyDown(menuOption, { key: "Tab" });

      expect(handleExit).toHaveBeenCalledTimes(0);
      element.unmount();
    });

    it("modal-exit-button keyDown enter", () => {
      const { handleExit, element } = renderModal();

      const menuOption = screen.queryAllByTestId("modal-exit-button")[0];

      fireEvent.keyDown(menuOption, { key: "Enter" });

      expect(handleExit).toHaveBeenCalledTimes(1);
      element.unmount();
    });

    it("modal-bg", () => {
      const { handleExit, element } = renderModal();

      screen.queryAllByTestId("modal-bg")[0].click();
      expect(handleExit).toHaveBeenCalled();
      element.unmount();
    });
  });

  describe("Open Modal", () => {
    function renderModal2(): [
      jest.Mock<any, any>,
      RenderResult<typeof import("@testing-library/dom/types/queries"), HTMLElement>
    ] {
      const spyScrollTo = jest.fn();
      window.scrollTo = spyScrollTo;

      const handleExit = jest.fn() as any;
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

    it("should open modal", () => {
      const [spyScrollTo, component] = renderModal2();

      const element = document.body.querySelector(".modal-bg--open");

      expect(element).toBeTruthy();

      component.unmount();

      spyScrollTo.mockClear();
    });
  });
});
