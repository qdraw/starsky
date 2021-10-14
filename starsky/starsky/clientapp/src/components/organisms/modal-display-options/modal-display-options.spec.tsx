import { globalHistory } from "@reach/router";
import { render, RenderResult } from "@testing-library/react";
import { ReactWrapper } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import * as Modal from "../../atoms/modal/modal";
import ModalDisplayOptions from "./modal-display-options";

describe("ModalDisplayOptions", () => {
  it("renders", () => {
    render(
      <ModalDisplayOptions isOpen={true} parentFolder="/" handleExit={() => {}}>
        test
      </ModalDisplayOptions>
    );
  });

  describe("with Context", () => {
    describe("buttons exist", () => {
      var modal: RenderResult;
      beforeEach(() => {
        modal = render(
          <ModalDisplayOptions
            parentFolder={"/"}
            isOpen={true}
            handleExit={() => {}}
          />
        );
      });

      afterAll(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
          modal.unmount();
        });
      });

      it("toggle-collections", () => {
        const toggleCollections = modal.queryByTestId("toggle-collections");
        console.log(modal.container.innerHTML);

        expect(toggleCollections).toBeTruthy();
      });
      it("toggle-slow-files", () => {
        expect(modal.exists('[data-test="toggle-slow-files"]')).toBeTruthy();
      });
    });

    describe("click button", () => {
      var modal: ReactWrapper;
      beforeEach(() => {
        jest.useFakeTimers();
        modal = render(
          <ModalDisplayOptions
            parentFolder={"/"}
            isOpen={true}
            handleExit={() => {}}
          />
        );
      });

      afterEach(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
          modal.unmount();
        });
        jest.useRealTimers();
      });

      it("toggle-collections", () => {
        modal
          .find('[data-test="toggle-collections"] input')
          .first()
          .simulate("change");

        expect(globalHistory.location.search).toBe("?collections=false");

        modal
          .find('[data-test="toggle-collections"] input')
          .last()
          .simulate("change");

        expect(globalHistory.location.search).toBe("?collections=true");
      });

      it("toggle-slow-files should set localStorage", () => {
        modal
          .find('[data-test="toggle-slow-files"] input')
          .first()
          .simulate("change");

        expect(localStorage.getItem("alwaysLoadImage")).toBe("true");

        modal
          .find('[data-test="toggle-slow-files"] input')
          .last()
          .simulate("change");

        expect(localStorage.getItem("alwaysLoadImage")).toBe("false");
      });

      it("toggle-sockets", () => {
        modal
          .find('[data-test="toggle-sockets"] input')
          .first()
          .simulate("change");

        expect(localStorage.getItem("use-sockets")).toBe("false");

        modal
          .find('[data-test="toggle-sockets"] input')
          .last()
          .simulate("change");

        expect(localStorage.getItem("use-sockets")).toBe(null);
      });

      it("sort - change to imageFormat", () => {
        globalHistory.location.search = "";

        modal
          .find('[data-test="sort"] select')
          .first()
          .simulate("change", { target: { value: "imageFormat" } });

        expect(globalHistory.location.search).toBe("?sort=imageFormat");
      });

      it("sort - change to fileName", () => {
        globalHistory.location.search = "";

        modal
          .find('[data-test="sort"] select')
          .first()
          .simulate("change", { target: { value: "fileName" } });

        expect(globalHistory.location.search).toBe("?sort=fileName");
      });
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      const handleExitSpy = jest.fn();

      const component = render(
        <ModalDisplayOptions
          parentFolder="/"
          isOpen={true}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
