import { createEvent, fireEvent, render, RenderResult, screen } from "@testing-library/react";
import { act } from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import localization from "../../../localization/localization.json";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalDatetime from "./modal-edit-datetime";

describe("ModalArchiveMkdir", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(<ModalDatetime isOpen={true} subPath="/" handleExit={() => {}}></ModalDatetime>);
  });
  describe("with Context", () => {
    beforeEach(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    });

    it("no date input", () => {
      const modal = render(<ModalDatetime subPath={"/test"} isOpen={true} handleExit={() => {}} />);

      expect(screen.getByTestId("modal-edit-datetime-non-valid")).toBeTruthy();
      const errorText = screen.getByTestId("modal-edit-datetime-non-valid").textContent;
      expect(errorText).toContain(localization.MessageErrorDatetime.en);

      // and button is disabled
      const submitButtonBefore = screen.queryByTestId(
        "modal-edit-datetime-btn-default"
      ) as HTMLButtonElement;
      expect(submitButtonBefore).not.toBeNull();

      expect(submitButtonBefore.disabled).toBeTruthy();

      modal.unmount();
    });

    it("example date no error dialog", () => {
      const modal = render(
        <ModalDatetime
          dateTime="2020-01-01T01:29:40"
          subPath={"/test"}
          isOpen={true}
          handleExit={() => {}}
        />
      );

      // no warning
      expect(screen.queryByTestId("modal-edit-datetime-non-valid")).toBeFalsy();

      const submitButtonBefore = screen.queryByTestId(
        "modal-edit-datetime-btn-default"
      ) as HTMLButtonElement;
      expect(submitButtonBefore).not.toBeNull();

      // and button is NOT disabled
      expect(submitButtonBefore.disabled).toBeFalsy();
      modal.unmount();
    });

    function fireEventOnFormControl(_modal: RenderResult, dataName: string, input: string) {
      const formControls = screen.queryAllByTestId("form-control");

      const year = formControls.find(
        (p) => p.getAttribute("data-name") === dataName
      ) as HTMLElement;
      // use focusout instead of .blur
      const blurEventYear = createEvent.focusOut(year, {
        textContent: input
      });

      year.innerHTML = input;
      fireEvent(year, blurEventYear);
    }

    it("change all options to valid options", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        data: null,
        statusCode: 200
      });
      const fetchPostSpy = jest
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

      fireEventOnFormControl(modal, "year", "1998");
      fireEventOnFormControl(modal, "month", "12");
      fireEventOnFormControl(modal, "date", "1");
      fireEventOnFormControl(modal, "hour", "13");
      fireEventOnFormControl(modal, "minute", "5");
      fireEventOnFormControl(modal, "sec", "5");

      await act(async () => {
        await screen.queryByTestId("modal-edit-datetime-btn-default")?.click();
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().prefix + "/api/update",
        "f=%2Ftest&datetime=1998-12-01T13%3A05%3A05"
      );
    });

    it("change all options to invalid options", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        data: null,
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
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

      fireEventOnFormControl(modal, "year", "");
      fireEventOnFormControl(modal, "month", "");
      fireEventOnFormControl(modal, "date", "");
      fireEventOnFormControl(modal, "hour", "");
      fireEventOnFormControl(modal, "minute", "");
      fireEventOnFormControl(modal, "sec", "");

      await act(async () => {
        await screen.queryByTestId("modal-edit-datetime-btn-default")?.click();
      });

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
      expect(screen.getByTestId("modal-edit-datetime-non-valid")).toBeTruthy();
      const errorText = screen.getByTestId("modal-edit-datetime-non-valid").textContent;
      expect(errorText).toContain(localization.MessageErrorDatetime.en);
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
        <ModalDatetime subPath="/test.jpg" isOpen={true} handleExit={handleExitSpy} />
      );

      expect(handleExitSpy).toHaveBeenCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
