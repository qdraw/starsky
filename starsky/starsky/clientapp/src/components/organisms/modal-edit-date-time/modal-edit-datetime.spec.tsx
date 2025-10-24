import { createEvent, fireEvent, render, RenderResult, screen } from "@testing-library/react";
import { act } from "react";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import localization from "../../../localization/localization.json";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalDatetime, { GetDates, UpdateDateTime } from "./modal-edit-datetime";

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

  describe("GetDates (theory)", () => {
    // [month, date, fullYear, hour, minute, seconds, expected]
    const cases: [
      number | undefined,
      number | undefined,
      number | undefined,
      number | undefined,
      number | undefined,
      number | undefined,
      string
    ][] = [
      [10, 23, 2025, 14, 5, 9, "2025-10-23T14:05:09"],
      [1, 1, 2020, 0, 0, 0, "2020-01-01T00:00:00"],
      [12, 31, 1999, 23, 59, 59, "1999-12-31T23:59:59"],
      [undefined, 23, 2025, 14, 5, 9, ""],
      [10, undefined, 2025, 14, 5, 9, ""],
      [10, 23, undefined, 14, 5, 9, ""],
      [10, 23, 2025, undefined, 5, 9, ""],
      [10, 23, 2025, 14, undefined, 9, ""],
      [10, 23, 2025, 14, 5, undefined, ""],
      [10, 23, 2025, 0, 0, 0, "2025-10-23T00:00:00"],
      [0, 23, 2025, 14, 5, 9, ""],
      [10, 0, 2025, 14, 5, 9, ""]
    ];

    it.each(cases)(
      "GetDates(%p, %p, %p, %p, %p, %p) should return '%s'",
      (
        month: number | undefined,
        date: number | undefined,
        fullYear: number | undefined,
        hour: number | undefined,
        minute: number | undefined,
        seconds: number | undefined,
        expected: string
      ) => {
        expect(
          GetDates(
            month as number | undefined,
            date as number | undefined,
            fullYear as number | undefined,
            hour as number | undefined,
            minute as number | undefined,
            seconds as number | undefined
          )
        ).toBe(expected);
      }
    );
  });

  describe("UpdateDateTime", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: [
        {
          status: IExifStatus.Ok,
          fileName: "rootfilename.jpg",
          fileIndexItem: {
            description: "",
            fileHash: undefined,
            fileName: "test.jpg",
            filePath: "/test.jpg",
            isDirectory: false,
            status: "Ok",
            tags: "",
            title: ""
          }
        }
      ]
    });

    it("should not call FetchPost if form is not enabled", () => {
      const handleExit = jest.fn();
      UpdateDateTime(false, "subpath", "2025-10-24T12:00:00", handleExit);
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);
      expect(fetchPostSpy).not.toHaveBeenCalled();
      expect(handleExit).not.toHaveBeenCalled();
    });

    it("should call FetchPost with correct params and call handleExit on success", async () => {
      const handleExit = jest.fn();
      const mockResult = { statusCode: 200, data: [{ test: "ok" }] };
      fetchPostSpy.mockResolvedValueOnce(mockResult);

      await UpdateDateTime(true, "subpath", "2025-10-24T12:00:00", handleExit);

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      const [url, body] = fetchPostSpy.mock.calls[0];
      expect(url).toBe("mocked-url");
      expect(body).toContain("f=subpath");
      expect(body).toContain("datetime=2025-10-24T12%3A00%3A00");
      // Wait for promise to resolve
      await waitImmediate();
      expect(handleExit).toHaveBeenCalledWith([{ test: "ok" }]);
    });

    it("should not call handleExit if statusCode is not 200", async () => {
      const handleExit = jest.fn();
      fetchPostSpy.mockResolvedValueOnce({ statusCode: 400, data: [] });
      await UpdateDateTime(true, "subpath", "2025-10-24T12:00:00", handleExit);
      await waitImmediate();
      expect(handleExit).not.toHaveBeenCalled();
    });
  });
});
