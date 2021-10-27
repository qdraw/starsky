import { globalHistory } from "@reach/router";
import { fireEvent, render, RenderResult } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as Modal from "../../atoms/modal/modal";
import ModalDisplayOptions from "./modal-display-options";

describe("ModalDisplayOptions", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalDisplayOptions isOpen={true} parentFolder="/" handleExit={() => {}}>
        test
      </ModalDisplayOptions>
    );
  });

  describe("with Context", () => {
    describe("buttons exist", () => {
      let modal: RenderResult;
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
        const modalDisplayOptions = modal.queryByTestId(
          "modal-display-options"
        );

        expect(modalDisplayOptions).toBeTruthy();
        expect(toggleCollections).toBeTruthy();
      });
      it("toggle-slow-files", () => {
        expect(modal.queryByTestId("toggle-slow-files")).toBeTruthy();
      });
    });

    describe("click button", () => {
      var modal: RenderResult;
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

      it("toggle-collections", async () => {
        const toggleCollections = modal.queryByTestId("toggle-collections");

        const toFalseButton = toggleCollections?.querySelectorAll("input")[1];

        await act(async () => {
          await toFalseButton?.click();
        });

        expect(globalHistory.location.search).toBe("?collections=false");

        const toTrueButton = toggleCollections?.querySelectorAll("input")[0];

        await act(async () => {
          await toTrueButton?.click();
        });

        expect(globalHistory.location.search).toBe("?collections=true");
      });

      it("toggle-slow-files should set localStorage", async () => {
        const toggleSlowFiles = modal.queryByTestId("toggle-slow-files");
        const toFalseButton = toggleSlowFiles?.querySelectorAll("input")[1];

        await act(async () => {
          await toFalseButton?.click();
        });

        expect(localStorage.getItem("alwaysLoadImage")).toBe("true");

        const toTrueButton = toggleSlowFiles?.querySelectorAll("input")[0];

        await act(async () => {
          await toTrueButton?.click();
        });

        expect(localStorage.getItem("alwaysLoadImage")).toBe("false");
      });

      it("toggle-sockets", async () => {
        const toggleSockets = modal.queryByTestId("toggle-sockets");
        const toFalseButton = toggleSockets?.querySelectorAll("input")[1];

        await act(async () => {
          await toFalseButton?.click();
        });

        expect(localStorage.getItem("use-sockets")).toBe("false");

        const toTrueButton = toggleSockets?.querySelectorAll("input")[0];

        await act(async () => {
          await toTrueButton?.click();
        });

        expect(localStorage.getItem("use-sockets")).toBe(null);
      });

      it("sort - change to imageFormat", async () => {
        globalHistory.location.search = "";

        const select = modal.queryByTestId("select") as HTMLSelectElement;
        expect(select).not.toBeNull();

        fireEvent.change(select, { target: { value: "imageFormat" } });

        expect(globalHistory.location.search).toBe("?sort=imageFormat");
      });

      it("sort - change to fileName", () => {
        globalHistory.location.search = "";

        const select = modal.queryByTestId("select") as HTMLSelectElement;
        expect(select).not.toBeNull();
        fireEvent.change(select, { target: { value: "fileName" } });

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
