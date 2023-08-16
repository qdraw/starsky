import { act, fireEvent, render } from "@testing-library/react";
import React from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Router } from "../../../router-app/router-app";
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
    render(<MenuDetailView state={state} dispatch={jest.fn()} />);
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
      const moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const move = component.queryByTestId("move");
      expect(move).toBeTruthy();
      move?.click();

      expect(moveModal).toBeCalledTimes(0);

      // reset afterwards
      act(() => {
        window.location.replace("/");
        component.unmount();
      });
    });

    it("readonly - rename click", () => {
      const renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const rename = component.queryByTestId("rename");
      expect(rename).toBeTruthy();
      rename?.click();

      expect(renameModal).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });

    it("readonly - trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const trash = component.queryByTestId("trash");
      expect(trash).toBeTruthy();
      trash?.click();

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
      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      fireEvent.click(anchor, {
        metaKey: true
      });

      expect(component.queryByTestId("preloader")).toBeNull();

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
      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      fireEvent.click(anchor, {
        metaKey: false
      });

      expect(component.queryByTestId("preloader")).not.toBeNull();

      component.unmount();
    });
  });

  describe("with Context", () => {
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

    it("as search Result button exist", () => {
      // add search query to url
      window.location.replace("/?t=test&p=0");

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      expect(anchor).toBeTruthy();
      expect(anchor.className).toContain("search");

      act(() => {
        // reset afterwards
        component.unmount();
        window.location.replace("/");
      });
    });

    it("as search Result button exist no ?t", () => {
      // add search query to url
      window.location.replace("/");

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      expect(anchor).toBeTruthy();
      expect(anchor.className).toContain("item--close");

      act(() => {
        // reset afterwards
        component.unmount();
        window.location.replace("/");
      });
    });

    it("last Edited change [true]", () => {
      window.location.replace("/?details=true");

      //  With updated LastEdited

      const updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem,
          lastEdited: new Date().toISOString()
        }
      };

      const component = render(
        <MenuDetailView state={updateState} dispatch={jest.fn()} />
      );

      expect(component.queryByTestId("menu-detail-view-autosave")).toBeTruthy();

      act(() => {
        // reset afterwards
        component.unmount();
        window.location.replace("/");
      });
    });

    it("last Edited change [false]", () => {
      act(() => {
        window.location.replace("/?details=true");
      });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      expect(component.queryByTestId("menu-detail-view-autosave")).toBeFalsy();

      act(() => {
        // reset afterwards
        component.unmount();
        window.location.replace("/");
      });
    });

    it("export click [menu]", () => {
      const exportModal = jest
        .spyOn(ModalExport, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const exportButton = component.queryByTestId("export");
      expect(exportButton).toBeTruthy();

      act(() => {
        exportButton?.click();
      });

      expect(exportModal).toBeCalled();

      // to avoid polling afterwards
      act(() => {
        component.unmount();
      });
    });

    it("labels click .item--labels [menu]", () => {
      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const labels = component.queryByTestId("menu-detail-view-labels");

      act(() => {
        labels?.click();
      });

      const urlObject = new URLPath().StringToIUrl(window.location.search);

      expect(urlObject.details).toBeTruthy();

      // dont keep any menus open
      act(() => {
        component.unmount();
        // reset afterwards
        window.location.replace("/");
      });
    });

    it("labels click (in MoreMenu)", () => {
      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      //       const item = component.find('[data-test="labels"]');
      const labels = component.queryByTestId("labels");
      expect(labels).toBeTruthy();
      labels?.click();

      const urlObject = new URLPath().StringToIUrl(
        Router.state.location.search
      );
      expect(urlObject.details).toBeTruthy();

      // reset afterwards
      act(() => {
        window.location.replace("/");
        component.unmount();
      });
    });

    it("navigate to parent folder click", () => {
      window.location.replace("/?t=test");

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const goToParentFolder = component.queryByTestId("go-to-parent-folder");
      expect(goToParentFolder).toBeTruthy();
      goToParentFolder?.click();

      expect(Router.state.location.search).toBe("?f=/test");

      act(() => {
        component.unmount();
        window.location.replace("/");
      });
    });

    it("[menu detail] move click", () => {
      const moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const move = component.queryByTestId("move");
      expect(move).toBeTruthy();

      act(() => {
        move?.click();
      });

      expect(moveModal).toBeCalled();

      // reset afterwards
      act(() => {
        window.location.replace("/");
        component.unmount();
      });
    });

    it("rename click", () => {
      const renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const rename = component.queryByTestId("rename");
      expect(rename).toBeTruthy();

      act(() => {
        rename?.click();
      });

      expect(renameModal).toBeCalled();

      act(() => {
        component.unmount();
      });
    });

    it("trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const trash = component.queryByTestId("trash");
      expect(trash).toBeTruthy();
      trash?.click();

      expect(spy).toBeCalled();
      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith(
        new UrlQuery().UrlMoveToTrashApi(),
        "f=%2Ftest%2Fimage.jpg"
      );

      act(() => {
        component.unmount();
      });
    });

    it("trash click to trash and collection is true", () => {
      const component = render(
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

      const trashIncl = component.queryByTestId("trash-including");

      expect(trashIncl).toBeTruthy();

      expect(trashIncl?.textContent).toBe("Including: jpg, arw");

      act(() => {
        component.unmount();
      });
    });

    it("rotate click", async () => {
      jest.useFakeTimers();
      const setTimeoutSpy = jest.spyOn(global, "setTimeout");

      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spyPost = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
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
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchGet, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      const item = component.queryByTestId("rotate");

      // need to await this click 2 times
      await act(async () => {
        await item?.click();
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
      const state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: {
          status: IExifStatus.Deleted,
          filePath: "/trashed/test1.jpg",
          fileName: "test1.jpg"
        }
      } as IDetailView;
      const contextValues = { state, dispatch: jest.fn() };

      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);

      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const trash = component.queryByTestId("trash");
      expect(trash).toBeTruthy();
      trash?.click();

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

      const state = {
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

      const component = render(
        <MenuDetailView state={state} dispatch={jest.fn()} />
      );

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spyFetchPost = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const event = new KeyboardEvent("keydown", {
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
      const state1 = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: {
          status: IExifStatus.Deleted,
          filePath: "/trashed/test1.jpg",
          fileName: "test1.jpg",
          lastEdited: ""
        }
      } as IDetailView;

      const component = render(
        <MenuDetailView state={state1} dispatch={jest.fn()} />
      );
      let header = component.container.querySelector(
        "header"
      ) as HTMLHeadingElement;
      expect(header.className).toBe("header header--main header--deleted");

      act(() => {
        state1.fileIndexItem.status = IExifStatus.Ok;
      });

      act(() => {
        window.location.replace("/?f=/test2.jpg");
      });

      header = component.container.querySelector(
        "header"
      ) as HTMLHeadingElement;

      expect(header.className).toBe("header header--main");

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

        const component = render(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );

        const item = component.queryByTestId("export") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved", () => {
        jest.spyOn(React, "useContext").mockReset();

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );

        const item = component.queryByTestId("move") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be renamed", () => {
        jest.spyOn(React, "useContext").mockReset();

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );

        const item = component.queryByTestId("rename") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved to trash", () => {
        jest.spyOn(React, "useContext").mockReset();

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );

        const item = component.queryByTestId("trash") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be rotated", () => {
        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MenuDetailView state={state} dispatch={jest.fn()} />
        );

        const item = component.queryByTestId("rotate") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });
    });
  });
});
