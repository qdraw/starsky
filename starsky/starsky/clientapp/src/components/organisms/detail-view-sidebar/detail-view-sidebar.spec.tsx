import { fireEvent, render, RenderResult, screen, waitFor } from "@testing-library/react";
import { act } from "react";
import { DetailViewContext } from "../../../contexts/detailview-context";
import * as useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView, IRelativeObjects, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { ClipboardHelper } from "../../../shared/clipboard-helper";
import { parseDate, parseTime } from "../../../shared/date";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { Keyboard } from "../../../shared/keyboard";
import { SupportedLanguages } from "../../../shared/language";
import * as ClearSearchCache from "../../../shared/search/clear-search-cache";
import { UrlQuery } from "../../../shared/url/url-query";
import { LimitLength } from "../../atoms/form-control/limit-length";
import * as ModalDatetime from "../modal-edit-date-time/modal-edit-datetime";
import DetailViewSidebar from "./detail-view-sidebar";

describe("DetailViewSidebar", () => {
  it("renders (without state component)", () => {
    render(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={{ fileIndexItem: { lastEdited: "" } } as unknown as IDetailView}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
  });

  beforeEach(() => {
    jest.spyOn(console, "error").mockImplementationOnce(() => {});
  });

  it("test warning (without state component)", () => {
    const wrapper = render(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={undefined as unknown as IDetailView}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
    const serverError = screen.queryByTestId("detailview-exifstatus-status-server-error");
    expect(serverError).not.toBeNull();

    wrapper.unmount();
  });

  describe("useContext-test", () => {
    let contextProvider: {
      dispatch: () => void;
      state: IDetailView;
    };
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
        } as unknown as IDetailView
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
      const tags = screen.queryByTestId("detailview-sidebar-tags");
      expect(tags?.textContent).toBe("tags!");
    });

    it("test if title from the context is displayed", () => {
      const tags = findDataName("title");
      expect(tags?.textContent).toBe("title!");
    });

    it("test if description from the context is displayed", () => {
      const description = findDataName("description");
      expect(description?.textContent).toBe("description!");
    });

    it("test if colorclass from the context is displayed", () => {
      const colorClassSelect = Component.queryByTestId("color-class-select");

      expect(colorClassSelect?.dataset.colorclass).toBe("3");
    });

    it("test if dateTime from the context is displayed", () => {
      const dateTime = Component.queryByTestId("dateTime");

      expect(dateTime?.textContent).toBe(
        parseDate("2019-09-15T17:29:59", SupportedLanguages.en) + parseTime("2019-09-15T17:29:59")
      );
    });

    it("click on datetime modal", () => {
      const dateTime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(dateTime).not.toBeNull();

      // import * as ModalDatetime from './modal-datetime';
      const modalDatetimeSpy = jest.spyOn(ModalDatetime, "default").mockImplementationOnce(() => {
        return <></>;
      });

      act(() => {
        dateTime.click();
      });

      expect(modalDatetimeSpy).toHaveBeenCalled();
    });

    it("click on datetime modal and return value", () => {
      const dateTime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(dateTime).not.toBeNull();

      // import * as ModalDatetime from './modal-datetime';
      const modalDatetimeSpy = jest
        .spyOn(ModalDatetime, "default")
        .mockImplementationOnce((props) => {
          props.handleExit([{ dateTime: "2020-02-01T13:15:20" }] as IFileIndexItem[]);
          return <></>;
        });

      act(() => {
        dateTime.click();
      });

      expect(modalDatetimeSpy).toHaveBeenCalled();

      const updatedDatetime = Component.queryByTestId("dateTime") as HTMLElement;
      expect(updatedDatetime).not.toBeNull();

      expect(updatedDatetime.textContent).toBe(
        parseDate("2020-02-01T13:15:20", SupportedLanguages.en) + parseTime("2020-02-01T13:15:20")
      );
    });

    it("click on ColorClassSelect and return value", () => {
      const colorClassSelectItem = Component.queryByTestId("color-class-select-5") as HTMLElement;

      act(() => {
        colorClassSelectItem.click();
      });

      const lastEdited = Component.queryByTestId("lastEdited") as HTMLElement;

      expect(lastEdited).not.toBeNull();
      expect(lastEdited.textContent).toBe("less than one minuteago edited");
    });

    it("test if lastEdited from the context is displayed", () => {
      const lastEdited = Component.queryByTestId("lastEdited") as HTMLElement;

      expect(lastEdited).not.toBeNull();
      expect(lastEdited.textContent).toBe("less than one minuteago edited");
    });

    it("test if make from the context is displayed", () => {
      const make = Component.queryByTestId("make") as HTMLElement;

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
      const locationDiv = Component.queryByTestId("detailview-location-div") as HTMLElement;

      expect(locationDiv).not.toBeNull();
    });

    it("On change a tag there is an API called", async () => {
      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = screen.queryByTestId("detailview-sidebar-tags");

      expect(tagsField).not.toBeNull();

      act(() => {
        tagsField!.innerHTML = "a";
      });

      fireEvent.blur(tagsField!, { currentTarget: tagsField });

      await waitFor(() => expect(fetchPostSpy).toHaveBeenCalled());

      const expectedBodyParams = new URLSearchParams();
      expectedBodyParams.append("f", "/test.jpg");
      expectedBodyParams.append("tags", "a");

      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlUpdateApi(),
        expectedBodyParams.toString()
      );

      fetchPostSpy.mockClear();
    });

    it("When there is nothing in the tags field a null char is send", () => {
      const nullChar = "\0";

      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = screen.queryByTestId("detailview-sidebar-tags") as HTMLInputElement;

      act(() => {
        tagsField.innerHTML = "";
      });

      fireEvent.blur(tagsField, { currentTarget: tagsField });

      expect(fetchPostSpy).toHaveBeenCalled();

      const expectedBodyParams = new URLSearchParams();
      expectedBodyParams.append("f", "/test.jpg");
      expectedBodyParams.append("tags", "\0");

      const expectedBodyString = expectedBodyParams.toString().replace(/%00/gi, nullChar);

      expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().UrlUpdateApi(), expectedBodyString);

      fetchPostSpy.mockClear();
    });

    it("search cache clear AND when a tag is updated", async () => {
      document.location.search = "/?t=test";

      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        statusCode: 200,
        data: [{ filePath: "/test.jpg" } as IFileIndexItem]
      });

      const clearSearchCacheSpy = jest.spyOn(ClearSearchCache, "ClearSearchCache");

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      const tagsField = screen.queryByTestId("detailview-sidebar-tags") as HTMLInputElement;

      // need to await here
      act(() => {
        tagsField.innerHTML = "a";
      });

      fireEvent.blur(tagsField, { currentTarget: tagsField });

      await waitFor(() => expect(clearSearchCacheSpy).toHaveBeenCalled());

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);

      jest.spyOn(LimitLength.prototype, "LimitLengthBlur").mockClear();

      document.location.search = "";
    });
  });

  describe("Special status", () => {
    let contextProvider: {
      dispatch: () => void;
      state: IDetailView;
    };

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
        } as unknown as IDetailView
      };
    });

    function findDataNameCurrent(_component: RenderResult, name: string) {
      return screen
        .queryAllByTestId("form-control")
        .find((p) => (p as HTMLElement).getAttribute("data-name") === name);
    }

    it("Deleted status (from FileIndexItem)", () => {
      contextProvider.state.fileIndexItem.status = IExifStatus.Deleted;

      const DeletedTestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Deleted}
            filePath={"/t"}
            state={contextProvider.state}
            dispatch={jest.fn()}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      const component = render(<DeletedTestComponent />) as RenderResult;

      const statusDeleted = component.queryByTestId("detailview-exifstatus-status-deleted");
      expect(statusDeleted).not.toBeNull();

      contextProvider.state.fileIndexItem.status = IExifStatus.Deleted;

      const tagsField = screen.queryByTestId("detailview-sidebar-tags") as HTMLInputElement;

      const description = findDataNameCurrent(component, "description");
      const title = findDataNameCurrent(component, "title");

      expect(tagsField?.classList).toContain("form-control");
      expect(tagsField?.classList).toContain("disabled");
      expect(description?.classList).toContain("disabled");
      expect(title?.classList).toContain("disabled");

      component.unmount();
    }, 14000);

    it("ReadOnly status (from FileIndexItem)", async () => {
      contextProvider.state.fileIndexItem.status = IExifStatus.ReadOnly;

      const DeletedTestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Ok}
            state={contextProvider.state}
            dispatch={jest.fn()}
            filePath={"/t"}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      const component = render(<DeletedTestComponent />) as RenderResult;

      const statusReadOnly = component.queryByTestId("detailview-exifstatus-status-read-only");
      expect(statusReadOnly).not.toBeNull();

      // Tags and other input fields are disabled
      const tags = screen.queryByTestId("detailview-sidebar-tags") as HTMLInputElement;
      const description = findDataNameCurrent(component, "description");
      const title = findDataNameCurrent(component, "title");

      await waitFor(() => expect(tags?.classList).toContain("form-control"));

      await waitFor(() => expect(tags?.classList).toContain("disabled"), {
        timeout: 8000
      });
      await waitFor(() => expect(description?.classList).toContain("disabled"));
      await waitFor(() => expect(title?.classList).toContain("disabled"));

      component.unmount();
    }, 13000);
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
    } as unknown as IDetailView;

    it("Press v to paste return false", () => {
      let vPasteIsCalled = false;

      function keyboardCallback(regex: RegExp, callback: (arg0: KeyboardEvent) => void) {
        if (regex.source === "^v$") {
          const event = new KeyboardEvent("keydown", {
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

      jest.spyOn(Keyboard.prototype, "SetFocusOnEndField").mockImplementationOnce(() => {});

      jest.spyOn(Keyboard.prototype, "isInForm").mockImplementationOnce(() => false);

      jest.spyOn(ClipboardHelper.prototype, "PasteAsync").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });

      const component = render(
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

    it("Press v to paste return true", () => {
      let vPasteIsCalled = false;

      function keyboardCallback(regex: RegExp, callback: (arg0: KeyboardEvent) => void) {
        if (regex.source === "^v$") {
          const event = new KeyboardEvent("keydown", {
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

      jest.spyOn(Keyboard.prototype, "SetFocusOnEndField").mockImplementationOnce(() => {});

      jest.spyOn(Keyboard.prototype, "isInForm").mockImplementationOnce(() => false);

      jest.spyOn(ClipboardHelper.prototype, "PasteAsync").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      const component = render(
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

      function keyboardCallback(regex: RegExp, callback: (arg0: KeyboardEvent) => void) {
        if (regex.source === "^c$") {
          const event = new KeyboardEvent("keydown", {
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

      jest.spyOn(Keyboard.prototype, "SetFocusOnEndField").mockImplementationOnce(() => {});

      jest.spyOn(Keyboard.prototype, "isInForm").mockImplementationOnce(() => false);

      jest.spyOn(ClipboardHelper.prototype, "Copy").mockImplementationOnce(() => {
        return false;
      });

      const component = render(
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
      const contextProvider = {
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
        } as unknown as IDetailView
      };

      const isInFormSpy = jest
        .spyOn(Keyboard.prototype, "isInForm")
        .mockReset()
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
        .mockImplementationOnce((_, callback) => {
          callback({
            preventDefault: () => {}
          } as unknown as KeyboardEvent);
        })
        .mockImplementationOnce(() => {});

      const TestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar
            status={IExifStatus.Ok}
            filePath={"/t"}
            state={contextProvider.state}
            dispatch={jest.fn()}
          ></DetailViewSidebar>
        </DetailViewContext.Provider>
      );

      const component = render(<TestComponent />, {
        baseElement: document.body
      });

      expect(useKeyboardEventSpy).toHaveBeenCalled();
      expect(isInFormSpy).toHaveBeenCalled();
      expect(isInFormSpy).toHaveBeenCalledTimes(1);

      // const event = new KeyboardEvent("keydown", {
      //   bubbles: true,
      //   cancelable: true,
      //   key: "t",
      //   shiftKey: true
      // });

      // await act(async () => {
      //   await window.dispatchEvent(event);
      // });

      // await waitFor(() => expect(keyboardSpy).toHaveBeenCalled());

      component.unmount();
    });
  });
});
