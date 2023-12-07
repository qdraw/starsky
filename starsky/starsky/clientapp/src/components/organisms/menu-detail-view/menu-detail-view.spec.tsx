import { act, fireEvent, render } from "@testing-library/react";
import React from "react";
import { BrowserRouter, MemoryRouter } from "react-router-dom";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Router } from "../../../router-app/router-app";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import * as Link from "../../atoms/link/link";
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
    render(
      <MemoryRouter>
        <MenuDetailView state={state} dispatch={jest.fn()} />
      </MemoryRouter>
    );
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const move = component.queryByTestId("move");
      expect(move).toBeTruthy();
      move?.click();

      expect(moveModal).toBeCalledTimes(0);

      // reset afterwards
      act(() => {
        Router.navigate("/");
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
      Router.navigate("/?t=test&p=0");

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      expect(anchor).toBeTruthy();
      expect(anchor.className).toContain("search");

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("as search Result button exist no ?t", () => {
      // add search query to url
      Router.navigate("/");

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const anchor = component.queryByTestId(
        "menu-detail-view-close"
      ) as HTMLAnchorElement;

      expect(anchor).toBeTruthy();
      expect(anchor.className).toContain("item--close");

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("last Edited change [true]", () => {
      Router.navigate("/?details=true");

      //  With updated LastEdited

      const updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem,
          lastEdited: new Date().toISOString()
        }
      };

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={updateState} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      expect(component.queryByTestId("menu-detail-view-autosave")).toBeTruthy();

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("last Edited change [false]", () => {
      act(() => {
        Router.navigate("/?details=true");
      });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      expect(component.queryByTestId("menu-detail-view-autosave")).toBeFalsy();

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("export click [menu]", () => {
      const exportModal = jest
        .spyOn(ModalExport, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const exportButton = component.queryByTestId("download");
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

    it("export keyDown [menu] Tab so ignore", () => {
      const exportModal = jest
        .spyOn(ModalExport, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const exportButton = component.queryByTestId("download") as HTMLElement;
      expect(exportButton).toBeTruthy();

      act(() => {
        fireEvent.keyDown(exportButton, { key: "Tab" });
      });

      expect(exportModal).toBeCalledTimes(0);

      // to avoid polling afterwards
      act(() => {
        component.unmount();
      });
    });

    it("export keyDown [menu] Enter so continue", () => {
      const exportModal = jest
        .spyOn(ModalExport, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const exportButton = component.queryByTestId("download") as HTMLElement;
      expect(exportButton).toBeTruthy();

      act(() => {
        fireEvent.keyDown(exportButton, { key: "Enter" });
      });

      expect(exportModal).toBeCalledTimes(1);

      // to avoid polling afterwards
      act(() => {
        component.unmount();
      });
    });

    it("labels click .item--labels [menu]", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId("menu-detail-view-labels");

      act(() => {
        labels?.click();
      });

      const urlObject = new URLPath().StringToIUrl(window.location.search);

      expect(urlObject.details).toBeTruthy();

      // don't keep any menus open
      act(() => {
        component.unmount();
        // reset afterwards
        Router.navigate("/");
      });
    });

    it("labels keyDown .item--labels [menu] Tab so ignore", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId(
        "menu-detail-view-labels"
      ) as HTMLElement;
      expect(labels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(labels, { key: "Tab" });
      });

      const urlObject = new URLPath().StringToIUrl(window.location.search);

      expect(urlObject.details).toBeFalsy();

      // don't keep any menus open
      act(() => {
        component.unmount();
        // reset afterwards
        Router.navigate("/");
      });
    });

    it("labels keyDown .item--labels [menu] Enter", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId(
        "menu-detail-view-labels"
      ) as HTMLElement;
      expect(labels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(labels, { key: "Enter" });
      });

      const urlObject = new URLPath().StringToIUrl(window.location.search);

      expect(urlObject.details).toBeTruthy();

      // don't keep any menus open
      act(() => {
        component.unmount();
        // reset afterwards
        Router.navigate("/");
      });
    });

    it("labels click (in MoreMenu)", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId("labels");
      expect(labels).toBeTruthy();

      act(() => {
        labels?.click();
      });

      const urlObject = new URLPath().StringToIUrl(
        Router.state.location.search
      );
      expect(urlObject.details).toBeTruthy();

      // reset afterwards
      act(() => {
        Router.navigate("/");
        component.unmount();
      });
    });

    it("labels keyDown enter (in MoreMenu)", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId("labels") as HTMLElement;
      expect(labels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(labels, { key: "Enter" });
      });

      const urlObject = new URLPath().StringToIUrl(
        Router.state.location.search
      );
      expect(urlObject.details).toBeTruthy();

      // reset afterwards
      act(() => {
        Router.navigate("/");
        component.unmount();
      });
    });

    it("labels keyDown tab so skip (in MoreMenu)", () => {
      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const labels = component.queryByTestId("labels") as HTMLElement;
      expect(labels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(labels, { key: "Tab" });
      });

      const urlObject = new URLPath().StringToIUrl(
        Router.state.location.search
      );
      expect(urlObject.details).toBeFalsy();

      // reset afterwards
      act(() => {
        Router.navigate("/");
        component.unmount();
      });
    });

    it("navigate to parent folder click", () => {
      Router.navigate("/?t=test");

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const goToParentFolder = component.queryByTestId("go-to-parent-folder");
      expect(goToParentFolder).toBeTruthy();

      goToParentFolder?.click();

      expect(Router.state.location.search).toBe("?f=/test");

      act(() => {
        component.unmount();
        Router.navigate("/");
      });
    });

    it("[menu detail] move click", () => {
      const moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const move = component.queryByTestId("move");
      expect(move).toBeTruthy();

      act(() => {
        move?.click();
      });

      expect(moveModal).toHaveBeenCalled();

      // reset afterwards
      act(() => {
        Router.navigate("/");
        component.unmount();
      });
    });

    it("[menu detail] move keyDown tab so ignore", () => {
      const moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const move = component.queryByTestId("move") as HTMLElement;
      expect(move).toBeTruthy();

      act(() => {
        fireEvent.keyDown(move, { key: "Tab" });
      });

      expect(moveModal).toBeCalledTimes(0);

      // reset afterwards
      act(() => {
        Router.navigate("/");
        component.unmount();
      });
    });

    it("[menu detail] move keyDown enter", () => {
      const moveModal = jest
        .spyOn(ModalMoveFile, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const move = component.queryByTestId("move") as HTMLElement;
      expect(move).toBeTruthy();

      act(() => {
        fireEvent.keyDown(move, { key: "Enter" });
      });

      expect(moveModal).toBeCalledTimes(1);

      // reset afterwards
      act(() => {
        Router.navigate("/");
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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

    it("rename keyDown tab so ignore", () => {
      const renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const rename = component.queryByTestId("rename") as HTMLElement;
      expect(rename).toBeTruthy();

      act(() => {
        fireEvent.keyDown(rename, { key: "Tab" });
      });

      expect(renameModal).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });

    it("rename keyDown enter so continue", () => {
      const renameModal = jest
        .spyOn(ModalDetailviewRenameFile, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const rename = component.queryByTestId("rename") as HTMLElement;
      expect(rename).toBeTruthy();

      act(() => {
        fireEvent.keyDown(rename, { key: "Enter" });
      });

      expect(renameModal).toBeCalled();

      act(() => {
        component.unmount();
      });
    });

    it("trash keyDown to trash so tab so skip", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const trash = component.queryByTestId("trash") as HTMLElement;
      expect(trash).toBeTruthy();

      act(() => {
        fireEvent.keyDown(trash, { key: "Tab" });
      });

      expect(spy).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });

    it("trash keyDown to trash enter continue", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      const trash = component.queryByTestId("trash") as HTMLElement;
      expect(trash).toBeTruthy();

      act(() => {
        fireEvent.keyDown(trash, { key: "Enter" });
      });

      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith(
        new UrlQuery().UrlMoveToTrashApi(),
        "f=%2Ftest%2Fimage.jpg"
      );

      act(() => {
        component.unmount();
      });
    });

    it("trash keyDown to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
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
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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

    it("rotate keyDown tab so skip", () => {
      jest.useFakeTimers();
      jest.spyOn(global, "setTimeout");

      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spyPost = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MemoryRouter>
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
        </MemoryRouter>
      );

      const rotate = component.queryByTestId("rotate") as HTMLElement;

      act(() => {
        fireEvent.keyDown(rotate, { key: "Tab" });
      });

      expect(rotate).toBeTruthy();

      expect(spyPost).toBeCalledTimes(0);

      act(() => {
        component.unmount();
      });
    });

    it("rotate keyDown enter", () => {
      jest.useFakeTimers();
      jest.spyOn(global, "setTimeout");

      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      const spyPost = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(
        <MemoryRouter>
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
        </MemoryRouter>
      );

      const rotate = component.queryByTestId("rotate") as HTMLElement;

      act(() => {
        fireEvent.keyDown(rotate, { key: "Enter" });
      });

      expect(rotate).toBeTruthy();

      expect(spyPost).toBeCalledTimes(1);

      act(() => {
        component.unmount();
      });
    });

    it("press click menu-detail-view-close-details button", () => {
      Router.navigate("/?details=true");

      const updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem
        }
      };

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={updateState} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      expect(Router.state.location.search).toBe("?details=true");

      const closeButton = component.queryByTestId(
        "menu-detail-view-close-details"
      ) as HTMLElement;
      expect(closeButton).toBeTruthy();

      closeButton?.click();

      expect(Router.state.location.search).toBe("?details=false");

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("press keyDown enter menu-detail-view-close-details button", () => {
      Router.navigate("/?details=true");

      const updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem
        }
      };

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={updateState} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      expect(Router.state.location.search).toBe("?details=true");

      const closeButton = component.queryByTestId(
        "menu-detail-view-close-details"
      ) as HTMLElement;
      expect(closeButton).toBeTruthy();

      act(() => {
        fireEvent.keyDown(closeButton, { key: "Enter" });
      });

      expect(Router.state.location.search).toBe("?details=false");

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
      });
    });

    it("press keyDown tab skip menu-detail-view-close-details button so skips", () => {
      Router.navigate("/?details=true");

      const updateState = {
        ...state,
        fileIndexItem: {
          ...state.fileIndexItem
        }
      };

      const component = render(
        <MemoryRouter>
          <MenuDetailView state={updateState} dispatch={jest.fn()} />
        </MemoryRouter>
      );

      expect(Router.state.location.search).toBe("?details=true");

      const closeButton = component.queryByTestId(
        "menu-detail-view-close-details"
      ) as HTMLElement;
      expect(closeButton).toBeTruthy();

      act(() => {
        fireEvent.keyDown(closeButton, { key: "Tab" });
      });

      // keep the same
      expect(Router.state.location.search).toBe("?details=true");

      act(() => {
        // reset afterwards
        component.unmount();
        Router.navigate("/");
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state} dispatch={jest.fn()} />
        </MemoryRouter>
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
        <MemoryRouter>
          <MenuDetailView state={state1} dispatch={jest.fn()} />
        </MemoryRouter>
      );
      let header = component.container.querySelector(
        "header"
      ) as HTMLHeadingElement;
      expect(header.className).toBe("header header--main header--deleted");

      act(() => {
        state1.fileIndexItem.status = IExifStatus.Ok;
      });

      act(() => {
        Router.navigate("/?f=/test2.jpg");
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

        jest
          .spyOn(Link, "default")
          .mockImplementationOnce(() => <></>)
          .mockImplementationOnce(() => <></>);

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MemoryRouter>
            <MenuDetailView state={state} dispatch={jest.fn()} />
          </MemoryRouter>
        );

        const item = component.queryByTestId("download") as HTMLDivElement;

        expect(item.parentElement?.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved", () => {
        jest.spyOn(React, "useContext").mockReset();
        jest
          .spyOn(Link, "default")
          .mockImplementationOnce(() => <></>)
          .mockImplementationOnce(() => <></>);

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <BrowserRouter>
            <MenuDetailView state={state} dispatch={jest.fn()} />
          </BrowserRouter>
        );

        const item = component.queryByTestId("move") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be renamed", () => {
        jest.spyOn(React, "useContext").mockReset();
        jest
          .spyOn(Link, "default")
          .mockImplementationOnce(() => <></>)
          .mockImplementationOnce(() => <></>);

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MemoryRouter>
            <MenuDetailView state={state} dispatch={jest.fn()} />
          </MemoryRouter>
        );

        const item = component.queryByTestId("rename") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be moved to trash", () => {
        jest.spyOn(React, "useContext").mockReset();
        jest
          .spyOn(Link, "default")
          .mockImplementationOnce(() => <></>)
          .mockImplementationOnce(() => <></>);

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MemoryRouter>
            <MenuDetailView state={state} dispatch={jest.fn()} />
          </MemoryRouter>
        );

        const item = component.queryByTestId("trash") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });

      it("when source is missing file can't be rotated", () => {
        jest
          .spyOn(Link, "default")
          .mockImplementationOnce(() => <></>)
          .mockImplementationOnce(() => <></>);

        const state = {
          subPath: "/trashed/test1.jpg",
          fileIndexItem: {
            status: IExifStatus.NotFoundSourceMissing,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        } as IDetailView;

        const component = render(
          <MemoryRouter>
            <MenuDetailView state={state} dispatch={jest.fn()} />
          </MemoryRouter>
        );

        const item = component.queryByTestId("rotate") as HTMLDivElement;
        expect(item.className).toBe("menu-option disabled");
      });
    });
  });
});
