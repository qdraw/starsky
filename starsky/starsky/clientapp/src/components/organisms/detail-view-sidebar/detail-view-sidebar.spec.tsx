import {
  act,
  fireEvent,
  render,
  RenderResult,
  waitFor
} from "@testing-library/react";
import React from "react";
import { DetailViewContext } from "../../../contexts/detailview-context";
import * as useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IRelativeObjects, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { ClipboardHelper } from "../../../shared/clipboard-helper";
import { parseDate, parseTime } from "../../../shared/date";
import * as FetchPost from "../../../shared/fetch-post";
import { Keyboard } from "../../../shared/keyboard";
import { SupportedLanguages } from "../../../shared/language";
import * as ClearSearchCache from "../../../shared/search/clear-search-cache";
import { UrlQuery } from "../../../shared/url-query";
import { LimitLength } from "../../atoms/form-control/limit-length";
import * as ModalDatetime from "../modal-edit-date-time/modal-edit-datetime";
import DetailViewSidebar from "./detail-view-sidebar";

describe("DetailViewSidebar", () => {
  it("renders (without state component)", () => {
    render(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={{ fileIndexItem: { lastEdited: "" } } as any}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
  });

  beforeEach(() => {
    jest.spyOn(console, "error").mockImplementationOnce(() => {});
  });

  it("test warning (without state component)", () => {
    var wrapper = render(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={undefined as any}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
    const serverError = wrapper.queryByTestId(
      "detailview-exifstatus-status-server-error"
    );
    expect(serverError).not.toBeNull();
  });

  describe("useContext-test", () => {
    let contextProvider: any;
    let TestComponent: () => JSX.Element;
    let Component: RenderResult;

    // Setup mock
    beforeEach(() => {
      contextProvider = {
        dispatch: () => jest.fn(),
        state: {
          breadcrumb: [],
          fileIndexItem: {
            filePath: "/test.jpg",
            status: IExifStatus.Ok,
            tags: "tags!",
            description: "description!",
            title: "title!",
            colorClass: 3,
            dateTime: "2019-09-15T17:29:59",
            lastEdited: new Date().toISOString(),
            make: "apple",
            model: "iPhone",
            aperture: 2,
            focalLength: 10,
            longitude: 1,
            latitude: 1
          } as IFileIndexItem,
          relativeObjects: {} as IRelativeObjects,
          subPath: "/",
          status: IExifStatus.Default,
          pageType: PageType.DetailView,
          colorClassActiveList: []
        } as any
      };

      TestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Default}
            filePath={"/t"}
            state={contextProvider.state}
            dispatch={jest.fn()}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      Component = render(<TestComponent />);
    });

    afterAll(() => {
      Component.unmount();
      TestComponent = () => <></>;
    });

    function findDataName(name: string) {
      return Component.queryAllByTestId("form-control").find(
        (p) => p.getAttribute("data-name") === name
      );
    }

    it("test if tags from the context is displayed", () => {
      var tags = findDataName("tags");
      expect(tags?.textContent).toBe("tags!");
    });

    it("test if title from the context is displayed", () => {
      var tags = findDataName("title");
      expect(tags?.textContent).toBe("title!");
    });

    it("test if description from the context is displayed", () => {
      var description = findDataName("description");
      expect(description?.textContent).toBe("description!");
    });

    it("test if colorclass from the context is displayed", () => {
      const colorClassSelect = Component.queryByTestId("color-class-select");

      expect(colorClassSelect?.dataset.colorclass).toBe("3");
    });

    it("test if dateTime from the context is displayed", () => {
      var dateTime = Component.queryByTestId("dateTime");

      expect(dateTime?.textContent).toBe(
        parseDate("2019-09-15T17:29:59", SupportedLanguages.en) +
          parseTime("2019-09-15T17:29:59")
      );
    });

    it("click on datetime modal", () => {
      var dateTime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(dateTime).not.toBeNull();

      // import * as ModalDatetime from './modal-datetime';
      var modalDatetimeSpy = jest
        .spyOn(ModalDatetime, "default")
        .mockImplementationOnce((props) => {
          return <></>;
        });

      act(() => {
        dateTime.click();
      });

      expect(modalDatetimeSpy).toBeCalled();
    });

    it("click on datetime modal and return value", () => {
      var dateTime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(dateTime).not.toBeNull();

      // import * as ModalDatetime from './modal-datetime';
      var modalDatetimeSpy = jest
        .spyOn(ModalDatetime, "default")
        .mockImplementationOnce((props) => {
          props.handleExit([
            { dateTime: "2020-02-01T13:15:20" }
          ] as IFileIndexItem[]);
          return <></>;
        });

      act(() => {
        dateTime.click();
      });

      expect(modalDatetimeSpy).toBeCalled();

      var updatedDatetime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(updatedDatetime).not.toBeNull();

      expect(updatedDatetime.textContent).toBe(
        parseDate("2020-02-01T13:15:20", SupportedLanguages.en) +
          parseTime("2020-02-01T13:15:20")
      );
    });

    it("click on ColorClassSelect and return value", () => {
      const colorClassSelectItem = Component.queryByTestId(
        "color-class-select-5"
      ) as HTMLElement;

      act(() => {
        colorClassSelectItem.click();
      });

      var lastEdited = Component.queryByTestId("lastEdited") as HTMLElement;

      expect(lastEdited).not.toBeNull();
      expect(lastEdited.textContent).toBe("less than one minuteago edited");
    });

    it("test if lastEdited from the context is displayed", () => {
      var lastEdited = Component.queryByTestId("lastEdited") as HTMLElement;

      expect(lastEdited).not.toBeNull();
      expect(lastEdited.textContent).toBe("less than one minuteago edited");
    });

    it("test if make from the context is displayed", () => {
      var make = Component.queryByTestId("make") as HTMLElement;

      expect(make).not.toBeNull();
      expect(make.textContent).toContain("apple"); // <= with space on end, so contain
    });

    it("test if model from the context is displayed", () => {
      const model = Component.queryByTestId("model") as HTMLElement;

      expect(model).not.toBeNull();
      expect(model.textContent).toBe("iPhone");
    });

    it("test if aperture from the context is displayed", () => {
      const aperture = Component.queryByTestId("aperture") as HTMLElement;

      expect(aperture).not.toBeNull();
      expect(aperture.textContent).toBe("2");
    });

    it("test if focalLength from the context is displayed", () => {
      const focalLength = Component.queryByTestId("focalLength") as HTMLElement;

      expect(focalLength).not.toBeNull();
      expect(focalLength.textContent).toBe("10.0");
    });

    it("test if lat/long icon from the context is displayed", () => {
      const locationDiv = Component.queryByTestId(
        "detailview-location-div"
      ) as HTMLElement;

      expect(locationDiv).not.toBeNull();
    });

    it("On change a tag there is an API called", async () => {
      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ ...newIConnectionDefault(), statusCode: 200 });
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = findDataName("tags") as HTMLElement;
      expect(tagsField).not.toBeNull();

      act(() => {
        tagsField.innerHTML = "a";
      });

      fireEvent.blur(tagsField, { currentTarget: tagsField });

      await waitFor(() => expect(fetchPostSpy).toBeCalled());

      var expectedBodyParams = new URLSearchParams();
      expectedBodyParams.append("f", "/test.jpg");
      expectedBodyParams.append("tags", "a");

      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        expectedBodyParams.toString()
      );

      fetchPostSpy.mockClear();
    });

    it("When there is nothing in the tags field a null char is send", () => {
      var nullChar = "\0";

      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ ...newIConnectionDefault(), statusCode: 200 });
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = findDataName("tags") as HTMLInputElement;

      act(() => {
        tagsField.innerHTML = "";
      });

      fireEvent.blur(tagsField, { currentTarget: tagsField });

      expect(fetchPostSpy).toBeCalled();

      var expectedBodyParams = new URLSearchParams();
      expectedBodyParams.append("f", "/test.jpg");
      expectedBodyParams.append("tags", "\0");

      var expectedBodyString = expectedBodyParams
        .toString()
        .replace(/%00/gi, nullChar);

      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        expectedBodyString
      );

      fetchPostSpy.mockClear();
    });

    it("Deleted status (from FileIndexItem)", async () => {
      contextProvider.state.fileIndexItem.status = IExifStatus.Deleted;

      var DeletedTestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Ok}
            filePath={"/t"}
            state={contextProvider.state}
            dispatch={jest.fn()}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      var component = render(<DeletedTestComponent />);

      const statusDeleted = component.queryByTestId(
        "detailview-exifstatus-status-deleted"
      );
      expect(statusDeleted).not.toBeNull();

      // Tags and other input fields are disabled
      const tags = findDataName("tags") as HTMLInputElement;
      const description = findDataName("description");
      const title = findDataName("title");

      await waitFor(() => expect(tags?.classList).toContain("form-control"));
      await waitFor(() => expect(tags?.classList).toContain("disabled"), {
        timeout: 2000
      });
      await waitFor(() => expect(description?.classList).toContain("disabled"));
      await waitFor(() => expect(title?.classList).toContain("disabled"));

      component.unmount();
    });

    it("ReadOnly status (from FileIndexItem)", async () => {
      contextProvider.state.fileIndexItem.status = IExifStatus.ReadOnly;

      var DeletedTestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Ok}
            state={contextProvider.state}
            dispatch={jest.fn()}
            filePath={"/t"}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      var component = render(<DeletedTestComponent />);

      const statusReadOnly = component.queryByTestId(
        "detailview-exifstatus-status-read-only"
      );
      expect(statusReadOnly).not.toBeNull();

      // Tags and other input fields are disabled
      const tags = findDataName("tags") as HTMLInputElement;
      const description = findDataName("description");
      const title = findDataName("title");

      await waitFor(() => expect(tags?.classList).toContain("form-control"));

      await waitFor(() => expect(tags?.classList).toContain("disabled"), {
        timeout: 2000
      });
      await waitFor(() => expect(description?.classList).toContain("disabled"));
      await waitFor(() => expect(title?.classList).toContain("disabled"));

      component.unmount();
    });

    it("search cache clear AND when a tag is updated", async () => {
      document.location.search = "/?t=test";

      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          ...newIConnectionDefault(),
          statusCode: 200,
          data: [{ filePath: "/test.jpg" } as IFileIndexItem]
        });

      const clearSearchCacheSpy = jest.spyOn(
        ClearSearchCache,
        "ClearSearchCache"
      );

      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = findDataName("tags") as HTMLInputElement;

      // need to await here
      act(() => {
        tagsField.innerHTML = "a";
      });

      fireEvent.blur(tagsField, { currentTarget: tagsField });

      await waitFor(() => expect(clearSearchCacheSpy).toBeCalled());

      expect(fetchPostSpy).toBeCalledTimes(1);

      jest.spyOn(LimitLength.prototype, "LimitLengthBlur").mockClear();

      document.location.search = "";
    });
  });

  describe("Copy paste", () => {
    const state = {
      breadcrumb: [],
      fileIndexItem: {
        filePath: "/test.jpg",
        status: IExifStatus.Ok
      } as IFileIndexItem,
      status: IExifStatus.Default,
      pageType: PageType.DetailView,
      colorClassActiveList: []
    } as any;

    it("Press v to paste", () => {
      let vPasteIsCalled = false;
      function keyboardCallback(regex: RegExp, callback: Function) {
        if (regex.source === "^([v])$") {
          var event = new KeyboardEvent("keydown", {
            bubbles: true,
            cancelable: true,
            key: "v"
          });
          vPasteIsCalled = true;
          callback(event);
        }
      }

      jest
        .spyOn(useKeyboardEvent, "default")
        .mockImplementationOnce(keyboardCallback)
        .mockImplementationOnce(keyboardCallback)
        .mockImplementationOnce(keyboardCallback);

      jest
        .spyOn(Keyboard.prototype, "SetFocusOnEndField")
        .mockImplementationOnce(() => {});

      jest
        .spyOn(Keyboard.prototype, "isInForm")
        .mockImplementationOnce(() => false);

      jest
        .spyOn(ClipboardHelper.prototype, "Paste")
        .mockImplementationOnce(() => {
          return false;
        });

      var component = render(
        <DetailViewSidebar
          status={IExifStatus.Default}
          filePath={"/t"}
          state={state}
          dispatch={jest.fn()}
        ></DetailViewSidebar>
      );

      expect(vPasteIsCalled).toBeTruthy();

      component.unmount();
    });

    it("Press c to copy", () => {
      let cCopyIsCalled = false;
      function keyboardCallback(regex: RegExp, callback: Function) {
        if (regex.source === "^([c])$") {
          var event = new KeyboardEvent("keydown", {
            bubbles: true,
            cancelable: true,
            key: "c"
          });
          cCopyIsCalled = true;
          callback(event);
        }
      }

      jest
        .spyOn(useKeyboardEvent, "default")
        .mockImplementationOnce(keyboardCallback)
        .mockImplementationOnce(keyboardCallback)
        .mockImplementationOnce(keyboardCallback);

      jest
        .spyOn(Keyboard.prototype, "SetFocusOnEndField")
        .mockImplementationOnce(() => {});

      jest
        .spyOn(Keyboard.prototype, "isInForm")
        .mockImplementationOnce(() => false);

      jest
        .spyOn(ClipboardHelper.prototype, "Copy")
        .mockImplementationOnce(() => {
          return false;
        });

      var component = render(
        <DetailViewSidebar
          status={IExifStatus.Default}
          filePath={"/t"}
          state={state}
          dispatch={jest.fn()}
        ></DetailViewSidebar>
      );

      expect(cCopyIsCalled).toBeTruthy();

      component.unmount();
    });
  });

  describe("own context", () => {
    it("keydown t/i should be fired", async () => {
      var contextProvider = {
        dispatch: () => jest.fn(),
        state: {
          breadcrumb: [],
          fileIndexItem: {
            filePath: "/test.jpg",
            status: IExifStatus.Ok
          } as IFileIndexItem,
          status: IExifStatus.Default,
          pageType: PageType.DetailView,
          colorClassActiveList: []
        } as any
      };

      const isInFormSpy = jest
        .spyOn(Keyboard.prototype, "isInForm")
        .mockImplementationOnce(() => false)
        .mockImplementationOnce(() => false)
        .mockImplementationOnce(() => false)
        .mockImplementationOnce(() => false);

      // const setFocusOnEndFieldSpy = jest
      //   .spyOn(Keyboard.prototype, "SetFocusOnEndField")
      //   .mockImplementationOnce(() => {});

      const useKeyboardEventSpy = jest
        .spyOn(useKeyboardEvent, "default")
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce((key, callback) => {
          callback({ preventDefault: () => {} });
        })
        .mockImplementationOnce(() => {});

      var TestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Ok}
            filePath={"/t"}
            state={contextProvider.state}
            dispatch={jest.fn()}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );

      var component = render(<TestComponent />, { baseElement: document.body });

      expect(useKeyboardEventSpy).toBeCalled();
      expect(isInFormSpy).toBeCalled();
      expect(isInFormSpy).toBeCalledTimes(1);

      // var event = new KeyboardEvent("keydown", {
      //   bubbles: true,
      //   cancelable: true,
      //   key: "t",
      //   shiftKey: true
      // });

      // await act(async () => {
      //   await window.dispatchEvent(event);
      // });

      // await waitFor(() => expect(keyboardSpy).toBeCalled());

      component.unmount();
    });
  });
});
