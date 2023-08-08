import {
  act,
  createEvent,
  fireEvent,
  render,
  RenderResult,
  screen,
} from "@testing-library/react";
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
      <ModalDatetime
        isOpen={true}
        subPath="/"
        handleExit={() => {}}
      ></ModalDatetime>,
    );
  });
  describe("with Context", () => {
    beforeEach(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    });

    it("no date input", () => {
      const modal = render(
        <ModalDatetime subPath={"/test"} isOpen={true} handleExit={() => {}} />,
      );

      expect(screen.getByTestId("modal-edit-datetime-non-valid")).toBeTruthy();

      // and button is disabled
      const submitButtonBefore = screen.queryByTestId(
        "modal-edit-datetime-btn-default",
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
        />,
      );

      // no warning
      expect(screen.queryByTestId("modal-edit-datetime-non-valid")).toBeFalsy();

      const submitButtonBefore = screen.queryByTestId(
        "modal-edit-datetime-btn-default",
      ) as HTMLButtonElement;
      expect(submitButtonBefore).not.toBeNull();

      // and button is NOT disabled
      expect(submitButtonBefore.disabled).toBeFalsy();
      modal.unmount();
    });

    async function fireEventOnFormControl(
      _modal: RenderResult,
      dataName: string,
      input: string,
    ) {
      const formControls = screen.queryAllByTestId("form-control");

      const year = formControls.find(
        (p) => p.getAttribute("data-name") === dataName,
      ) as HTMLElement;
      // use focusout instead of .blur
      const blurEventYear = createEvent.focusOut(year, {
        textContent: input,
      });

      year.innerHTML = input;
      await fireEvent(year, blurEventYear);
    }

    it("change all options", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ data: null, statusCode: 200 });
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
        />,
      );

      await fireEventOnFormControl(modal, "year", "1998");
      await fireEventOnFormControl(modal, "month", "12");
      await fireEventOnFormControl(modal, "date", "1");
      await fireEventOnFormControl(modal, "hour", "13");
      await fireEventOnFormControl(modal, "minute", "5");
      await fireEventOnFormControl(modal, "sec", "5");

      await act(async () => {
        await screen.queryByTestId("modal-edit-datetime-btn-default")?.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().prefix + "/api/update",
        "f=%2Ftest&datetime=1998-12-01T13%3A05%3A05",
      );
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
        <ModalDatetime
          subPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
        />,
      );

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
