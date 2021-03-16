import { globalHistory, Link } from "@reach/router";
import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import * as ModalDetailviewRenameFile from "../modal-detailview-rename-file/modal-detailview-rename-file";
import * as ModalExport from "../modal-download/modal-download";
import * as ModalMoveFile from "../modal-move-file/modal-move-file";
import MenuDetailView from "./menu-detail-view";

describe("MenuDetailView", () => {
  it("renders", () => {
    const state = {
      subPath: "/test/image.jpg",
      isReadOnly: true,
      fileIndexItem: {
        status: IExifStatus.Ok,
        fileHash: "000",
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        lastEdited: new Date(1970, 1, 1).toISOString(),
        parentDirectory: "/test"
      }
    } as IDetailView;
    shallow(<MenuDetailView state={state} dispatch={jest.fn()} />);
  });

  describe("readonly status context", () => {
    const state = {
      subPath: "/test/image.jpg",
      isReadOnly: true,
      fileIndexItem: {
        status: IExifStatus.Ok,
        fileHash: "000",
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        lastEdited: new Date(1970, 1, 1).toISOString(),
        parentDirectory: "/test"
      }
    } as IDetailView;

    it("readonly - move click", () => {
      var moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );

      var item = component.find('[data-test="move"]');

      act(() => {
        item.simulate("click");
      });

      expect(moveModal).toBeCalledTimes(0);

      // reset afterwards
      act(() => {
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("readonly - rename click", () => {
      var renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="rename"]');

      act(() => {
        item.simulate("click");
      });

      expect(renameModal).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });

    it("readonly - trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200 } as IConnectionDefault
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );
      var item = component.find('[data-test="trash"]');

      act(() => {
        item.simulate("click");
      });

      expect(fetchPostSpy).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });
  });

  describe("close", () => {
    it("when click on Link, with command key it should ignore preloader", () => {
      const state = {
        subPath: "/test/image.jpg",
        fileIndexItem: {
          status: IExifStatus.Ok,
          fileHash: "000",
          filePath: "/test/image.jpg",
          fileName: "image.jpg",
          lastEdited: new Date(1970, 1, 1).toISOString(),
          parentDirectory: "/test"
        }
      } as IDetailView;
      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );
      component.find(Link).simulate("click", {
        metaKey: true
      });

      expect(component.exists(".preloader--overlay")).toBeFalsy();
      component.unmount();
    });

    it("when click on Link, should show preloader", () => {
      const state = {
        subPath: "/test/image.jpg",
        fileIndexItem: {
          status: IExifStatus.Ok,
          fileHash: "000",
          filePath: "/test/image.jpg",
          fileName: "image.jpg",
          lastEdited: new Date(1970, 1, 1).toISOString(),
          parentDirectory: "/test"
        }
      } as IDetailView;
      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );
      component.find(Link).simulate("click");

      expect(component.exists(".preloader--overlay")).toBeTruthy();
      component.unmount();
    });
  });

  describe("with Context", () => {
    var state = {
      subPath: "/test/image.jpg",
      fileIndexItem: {
        status: IExifStatus.Ok,
        fileHash: "000",
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        lastEdited: new Date(1970, 1, 1).toISOString(),
        parentDirectory: "/test"
      }
    } as IDetailView;

    it("as search Result button exist", () => {
      // add search query to url
      globalHistory.navigate("/?t=test&p=0");

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );

      expect(component.exists(".item--search")).toBeTruthy();

      act(() => {
        // reset afterwards
        component.unmount();
        globalHistory.navigate("/");
      });
    });

    it("last Edited change [true]", () => {
      globalHistory.navigate("/?details=true");

      //  With updated LastEdited

      var updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem,
          lastEdited: new Date().toISOString()
        }
      };

      var component = mount(
        <MenuDetailView state={updateState} dispatch={jest.fn()} />
      );

      expect(component.exists(".autosave")).toBeTruthy();

      act(() => {
        // reset afterwards
        component.unmount();
        globalHistory.navigate("/");
      });
    });

    it("last Edited change [false]", () => {
      act(() => {
        globalHistory.navigate("/?details=true");
      });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()}>
          t
        </MenuDetailView>
      );

      expect(component.exists(".autosave")).toBeFalsy();

      act(() => {
        // reset afterwards
        component.unmount();
        globalHistory.navigate("/");
      });
    });

    it("export click [menu]", () => {
      var exportModal = jest
        .spyOn(ModalExport, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      var item = component.find('[data-test="export"]');

      act(() => {
        item.simulate("click");
      });

      expect(exportModal).toBeCalled();

      // to avoid polling afterwards
      act(() => {
        component.unmount();
      });
    });

    it("labels click .item--labels [menu]", () => {
      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      var find = component.find(".item.item--labels");

      act(() => {
        find.simulate("click");
      });

      var urlObject = new URLPath().StringToIUrl(globalHistory.location.search);

      expect(urlObject.details).toBeTruthy();

      // dont keep any menus open
      act(() => {
        component.unmount();
        // reset afterwards
        globalHistory.navigate("/");
      });
    });

    it("labels click (in MoreMenu)", () => {
      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="labels"]');

      act(() => {
        item.simulate("click");
      });

      var urlObject = new URLPath().StringToIUrl(globalHistory.location.search);
      expect(urlObject.details).toBeTruthy();

      // reset afterwards
      act(() => {
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("navigate to parent folder click", () => {
      globalHistory.navigate("/?t=test");

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="go-to-parent-folder"]');

      act(() => {
        item.simulate("click");
      });

      expect(globalHistory.location.search).toBe("?f=/test");

      act(() => {
        component.unmount();
        globalHistory.navigate("/");
      });
    });

    it("move click", () => {
      var moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      var item = component.find('[data-test="move"]');

      act(() => {
        item.simulate("click");
      });

      expect(moveModal).toBeCalled();

      // reset afterwards
      act(() => {
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("rename click", () => {
      var renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="rename"]');

      act(() => {
        item.simulate("click");
      });

      expect(renameModal).toBeCalled();

      act(() => {
        component.unmount();
      });
    });

    it("trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200 } as IConnectionDefault
      );
      var spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="trash"]');

      act(() => {
        item.simulate("click");
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        "f=%2Ftest%2Fimage.jpg&Tags=%21delete%21&append=true"
      );

      act(() => {
        component.unmount();
      });
    });

    it("trash click to trash and collection is true", () => {
      var component = mount(
        <MenuDetailView
          state={{
            ...state,
            fileIndexItem: {
              ...state.fileIndexItem,
              collectionPaths: [".jpg", "t.arw"]
            },
            collections: true
          }}
          dispatch={jest.fn()}
        />
      );
      expect(component.exists('[data-test="trash-including"]')).toBeTruthy();

      var trashIncl = component.find('[data-test="trash-including"]').text();

      expect(trashIncl).toBe("Including: jpg, arw");

      act(() => {
        component.unmount();
      });
    });

    it("rotate click", async () => {
      jest.useFakeTimers();
      var setTimeoutSpy = jest.spyOn(global, "setTimeout");

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200 } as IConnectionDefault
      );
      var spyPost = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        {
          statusCode: 200,
          data: {
            subPath: "/test/image.jpg",
            pageType: PageType.DetailView,
            fileIndexItem: {
              fileHash: "needed",
              status: IExifStatus.Ok,
              filePath: "/test/image.jpg",
              fileName: "image.jpg"
            }
          } as IDetailView
        } as IConnectionDefault
      );
      var spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      var component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );
      var item = component.find('[data-test="rotate"]');

      // need to await this click 2 times
      await act(async () => {
        await item.simulate("click");
      });

      expect(spyPost).toBeCalled();
      expect(spyPost).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        "f=%2Ftest%2Fimage.jpg&rotateClock=1"
      );

      act(() => {
        jest.advanceTimersByTime(3000);
      });

      expect(setTimeoutSpy).toBeCalled();
      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledWith(
        new UrlQuery().UrlIndexServerApi({ f: "/test/image.jpg" })
      );

      // cleanup afterwards
      act(() => {
        component.unmount();
        jest.useRealTimers();
      });
    });
  });

  describe("file is marked as deleted", () => {
    it("trash click to trash", () => {
      var state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: {
          status: IExifStatus.Deleted,
          filePath: "/trashed/test1.jpg",
          fileName: "test1.jpg"
        }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() };

      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      const component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200 } as IConnectionDefault
      );

      var spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var item = component.find('[data-test="trash"]');

      act(() => {
        item.simulate("click");
      });

      expect(spy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "f=%2Ftrashed%2Ftest1.jpg&fieldName=tags&search=%21delete%21"
      );

      // for some reason the spy is called 2 times here?
      act(() => {
        component.unmount();
      });
    });

    //  file is marked as deleted › press 'Delete' on keyboard to trash
    it("press 'Delete' on keyboard to trash", () => {
      jest.spyOn(FetchPost, "default").mockReset();

      var state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: {
          status: IExifStatus.Deleted,
          filePath: "/trashed/test1.jpg",
          fileName: "test1.jpg"
        },
        relativeObjects: {
          nextFilePath: "/",
          prevFilePath: "/"
        }
      } as IDetailView;

      const component = mount(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200 } as IConnectionDefault
      );
      var spyFetchPost = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var event = new KeyboardEvent("keydown", {
        bubbles: false,
        cancelable: true,
        key: "Delete",
        shiftKey: false,
        repeat: false
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(spyFetchPost).toBeCalled();
      expect(spyFetchPost).toBeCalledTimes(1);
      expect(spyFetchPost).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "f=%2Ftrashed%2Ftest1.jpg&fieldName=tags&search=%21delete%21"
      );

      act(() => {
        component.unmount();
      });
    });

    it("navigate to next item and reset some states", () => {
      let state1 = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: {
          status: IExifStatus.Deleted,
          filePath: "/trashed/test1.jpg",
          fileName: "test1.jpg",
          lastEdited: ""
        }
      } as IDetailView;

      const component = mount(
        <MenuDetailView state={state1} dispatch={jest.fn()} />
      );
      expect(component.find("header").getDOMNode().className).toBe(
        "header header--main header--deleted"
      );

      act(() => {
        state1.fileIndexItem.status = IExifStatus.Ok;
      });

      act(() => {
        globalHistory.navigate("/?f=/test2.jpg");
      });

      expect(component.find("header").getDOMNode().className).toBe(
        "header header--main"
      );

      act(() => {
        component.unmount();
      });
    });

    describe("NotFoundSourceMissing", () => {
      it("when source is missing file can't be downloaded", () => {
        jest.spyOn(React, "useContext").mockReset();

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = mount(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );
        var item = component.find('[data-test="export"]');

        expect(item.getDOMNode().className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved", () => {
        jest.spyOn(React, "useContext").mockReset();

        var state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = mount(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );
        var item = component.find('[data-test="move"]');

        expect(item.getDOMNode().className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be renamed", () => {
        jest.spyOn(React, "useContext").mockReset();

        var state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = mount(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );
        var item = component.find('[data-test="rename"]');

        expect(item.getDOMNode().className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved to trash", () => {
        jest.spyOn(React, "useContext").mockReset();

        var state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = mount(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );
        var item = component.find('[data-test="trash"]');

        expect(item.getDOMNode().className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be rotated", () => {
        var state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = mount(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );
        var item = component.find('[data-test="rotate"]');

        expect(item.getDOMNode().className).toBe("menu-option disabled");
      });
    });
  });
});
