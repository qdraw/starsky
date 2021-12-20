import {
  act,
  createEvent,
  fireEvent,
  render,
  RenderResult
} from "@testing-library/react";
import React from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalDatetime from "./modal-edit-datetime";

describe("ModalArchiveMkdir", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalDatetime isOpen={true} subPath="/" handleExit={() => {}}>
        test
      </ModalDatetime>
    );
  });
  describe("with Context", () => {
    beforeEach(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    });

    it("no date input", () => {
      var modal = render(
        <ModalDatetime subPath={"/test"} isOpen={true} handleExit={() => {}} />
      );

      expect(modal.queryByTestId("modal-edit-datetime-non-valid")).toBeTruthy();

      // and button is disabled
      const submitButtonBefore = modal.queryByTestId(
        "modal-edit-datetime-btn-default"
      ) as HTMLButtonElement;
      expect(submitButtonBefore).not.toBeNull();

      expect(submitButtonBefore.disabled).toBeTruthy();
    });

    it("example date no error dialog", () => {
      var modal = render(
        <ModalDatetime
          dateTime="2020-01-01T01:29:40"
          subPath={"/test"}
          isOpen={true}
          handleExit={() => {}}
        />
      );

      // no warning
      expect(modal.queryByTestId("modal-edit-datetime-non-valid")).toBeFalsy();

      const submitButtonBefore = modal.queryByTestId(
        "modal-edit-datetime-btn-default"
      ) as HTMLButtonElement;
      expect(submitButtonBefore).not.toBeNull();

      // and button is NOT disabled
      expect(submitButtonBefore.disabled).toBeFalsy();
    });

    async function fireEventOnFormControl(
      modal: RenderResult,
      dataName: string,
      input: string
    ) {
      const formControls = modal.queryAllByTestId("form-control");

      const year = formControls.find(
        (p) => p.getAttribute("data-name") === dataName
      ) as HTMLElement;
      // use focusout instead of .blur
      const blurEventYear = createEvent.focusOut(year, {
        textContent: input
      });

      await act(async () => {
        year.innerHTML = input;
        await fireEvent(year, blurEventYear);
      });
    }

    it("change all options", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ data: null, statusCode: 200 });
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const modal = render(
        <ModalDatetime
          dateTime="2020-01-01T01:29:40"
          subPath={"/test"}
          isOpen={true}
          handleExit={() => {
            // done();
          }}
        />
      );

      await fireEventOnFormControl(modal, "year", "1998");
      await fireEventOnFormControl(modal, "month", "12");
      await fireEventOnFormControl(modal, "date", "1");
      await fireEventOnFormControl(modal, "hour", "13");
      await fireEventOnFormControl(modal, "minute", "5");
      await fireEventOnFormControl(modal, "sec", "5");

      await act(async () => {
        await modal.queryByTestId("modal-edit-datetime-btn-default")?.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().prefix + "/api/update",
        "f=%2Ftest&datetime=1998-12-01T13%3A05%3A05"
      );
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      var handleExitSpy = jest.fn();

      var component = render(
        <ModalDatetime
          subPath="/test.jpg"
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
