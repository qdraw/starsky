import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
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
import * as ModalDatetime from "../modal-edit-date-time/modal-edit-datetime";
import DetailViewSidebar from "./detail-view-sidebar";

describe("DetailViewSidebar", () => {
  it("renders (without state component)", () => {
    shallow(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={{ fileIndexItem: { lastEdited: "" } } as any}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
  });

  it("test warning (without state component)", () => {
    var wrapper = mount(
      <DetailViewSidebar
        status={IExifStatus.Default}
        filePath={"/t"}
        state={undefined as any}
        dispatch={jest.fn()}
      ></DetailViewSidebar>
    );
    expect(
      wrapper.find(".detailview-sidebar").find(".warning-box")
    ).toHaveLength(1);
  });

  describe("useContext-test", () => {
    let contextProvider: any;
    let TestComponent: () => JSX.Element;
    let Component: ReactWrapper<any, Readonly<{}>>;

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
      Component = mount(<TestComponent />);
    });

    afterAll(() => {
      Component.unmount();
      TestComponent = () => <></>;
    });

    it("test if tags from the context is displayed", () => {
      var tags = Component.find('[data-name="tags"]');
      expect(tags.text()).toBe("tags!");
    });

    it("test if title from the context is displayed", () => {
      var title = Component.find('[data-name="title"]');
      expect(title.text()).toBe("title!");
    });

    it("test if description from the context is displayed", () => {
      var description = Component.find('[data-name="description"]');
      expect(description.text()).toBe("description!");
    });

    it("test if colorclass from the context is displayed", () => {
      expect(Component.exists(".colorclass--3.active")).toBeTruthy();
      // the rest is false
      expect(Component.exists(".colorclass--1.active")).toBeFalsy();
      expect(Component.exists(".colorclass--2.active")).toBeFalsy();
      expect(Component.exists(".colorclass--4.active")).toBeFalsy();
      expect(Component.exists(".colorclass--5.active")).toBeFalsy();
      expect(Component.exists(".colorclass--6.active")).toBeFalsy();
      expect(Component.exists(".colorclass--7.active")).toBeFalsy();
      expect(Component.exists(".colorclass--8.active")).toBeFalsy();
    });

    it("test if dateTime from the context is displayed", () => {
      var dateTime = Component.find('[data-test="dateTime"]');

      expect(dateTime.text()).toBe(
        parseDate("2019-09-15T17:29:59", SupportedLanguages.en) +
          parseTime("2019-09-15T17:29:59")
      );
    });

    it("click on datetime modal", () => {
      var dateTime = Component.find('[data-test="dateTime"]');

      // import * as ModalDatetime from './modal-datetime';
      var modalDatetimeSpy = jest
        .spyOn(ModalDatetime, "default")
        .mockImplementationOnce((props) => {
          return <></>;
        });

      act(() => {
        dateTime.simulate("click");
      });

      expect(modalDatetimeSpy).toBeCalled();
    });

    it("click on datetime modal and return value", () => {
      var dateTime = Component.find('[data-test="dateTime"]');

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
        dateTime.simulate("click");
      });

      expect(modalDatetimeSpy).toBeCalled();

      var updatedDatetime = Component.find('[data-test="dateTime"]');

      expect(updatedDatetime.text()).toBe(
        parseDate("2020-02-01T13:15:20", SupportedLanguages.en) +
          parseTime("2020-02-01T13:15:20")
      );
    });

    it("click on ColorClassSelect and return value", () => {
      var colorClassSelectItem = Component.find(".colorclass--5");

      act(() => {
        colorClassSelectItem.simulate("click");
      });

      var lastEdited = Component.find('[data-test="lastEdited"]');
      expect(lastEdited.text()).toBe("less than one minuteago edited");
    });

    it("test if lastEdited from the context is displayed", () => {
      var lastEdited = Component.find('[data-test="lastEdited"]');
      expect(lastEdited.text()).toBe("less than one minuteago edited");
    });

    it("test if make from the context is displayed", () => {
      var description = Component.find('[data-test="make"]');
      expect(description.text()).toContain("apple"); // <= with space on end
    });

    it("test if model from the context is displayed", () => {
      var description = Component.find('[data-test="model"]');
      expect(description.text()).toBe("iPhone");
    });

    it("test if aperture from the context is displayed", () => {
      var description = Component.find('[data-test="aperture"]');
      expect(description.text()).toBe("2");
    });

    it("test if focalLength from the context is displayed", () => {
      var description = Component.find('[data-test="focalLength"]');
      expect(description.text()).toBe("10.0");
    });

    it("test if lat/long icon from the context is displayed", () => {
      expect(Component.exists(".icon--location")).toBeTruthy();
    });

    it("On change a tag there is an API called", () => {
      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { ...newIConnectionDefault(), statusCode: 200 }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var tagsField = Component.find('[data-name="tags"]');

      act(() => {
        tagsField.getDOMNode().textContent = "a";
        tagsField.simulate("blur");
      });

      expect(fetchPostSpy).toBeCalled();

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
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { ...newIConnectionDefault(), statusCode: 200 }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var tagsField = Component.find('[data-name="tags"]');

      act(() => {
        tagsField.getDOMNode().textContent = "";
        tagsField.simulate("blur");
      });

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

    it("Deleted status (from FileIndexItem)", () => {
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
      var component = mount(<DeletedTestComponent />);

      expect(component.exists(".warning-box")).toBeTruthy();

      // Tags and other input fields are disabled
      expect(
        component.find('[data-name="tags"]').hasClass("disabled")
      ).toBeTruthy();
      expect(
        component.find('[data-name="description"]').hasClass("disabled")
      ).toBeTruthy();
      expect(
        component.find('[data-name="title"]').hasClass("disabled")
      ).toBeTruthy();

      component.unmount();
    });

    it("ReadOnly status (from FileIndexItem)", () => {
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
      var component = mount(<DeletedTestComponent />);

      expect(component.exists(".warning-box")).toBeTruthy();

      // Tags and other input fields are disabled
      expect(
        component.find('[data-name="tags"]').hasClass("disabled")
      ).toBeTruthy();
      expect(
        component.find('[data-name="description"]').hasClass("disabled")
      ).toBeTruthy();
      expect(
        component.find('[data-name="title"]').hasClass("disabled")
      ).toBeTruthy();
      component.unmount();
    });

    it("search cache clear AND when a tag is updated", async () => {
      document.location.search = "/?t=test";

      // spy on fetch
      // use this => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        {
          ...newIConnectionDefault(),
          statusCode: 200,
          data: [{ filePath: "/test.jpg" } as IFileIndexItem]
        }
      );

      const clearSearchCacheSpy = jest.spyOn(
        ClearSearchCache,
        "ClearSearchCache"
      );

      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      var tagsField = Component.find('[data-name="tags"]');

      // need to await here
      await act(async () => {
        tagsField.getDOMNode().textContent = "a";
        await tagsField.simulate("blur");
      });

      expect(clearSearchCacheSpy).toBeCalled();

      expect(fetchPostSpy).toBeCalledTimes(1);

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

      var component = mount(
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

      var component = mount(
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
    it("keydown t/i should be fired", () => {
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

      var keyboardSpy = jest
        .spyOn(Keyboard.prototype, "SetFocusOnEndField")
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {});

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
      var component = mount(<DeletedTestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "t",
        shiftKey: true
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(keyboardSpy).toBeCalled();

      component.unmount();
    });
  });
});
