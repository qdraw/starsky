import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import React, { act } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IArchive } from "../../../interfaces/IArchive";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as DropArea from "../../atoms/drop-area/drop-area";
import * as Link from "../../atoms/link/link";
import * as MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import * as ModalArchiveMkdir from "../modal-archive-mkdir/modal-archive-mkdir";
import * as ModalArchiveRename from "../modal-archive-rename/modal-archive-rename";
import { IModalRenameFolderProps } from "../modal-archive-rename/modal-archive-rename";
import * as ModalArchiveSynchronizeManually from "../modal-archive-synchronize-manually/modal-archive-synchronize-manually";
import * as ModalDisplayOptions from "../modal-display-options/modal-display-options";
import * as ModalDownload from "../modal-download/modal-download";
import * as ModalPublish from "../modal-publish/modal-publish";
import MenuArchive from "./menu-archive";

describe("MenuArchive", () => {
  it("renders", () => {
    render(<MenuArchive />);
  });

  describe("with Context", () => {
    beforeEach(() => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      jest
        .spyOn(window, "scrollTo")
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {});
    });

    it("default menu", () => {
      Router.navigate("/");

      const component = render(<MenuArchive />);

      expect(screen.getByTestId("hamburger")).toBeTruthy();
      expect(screen.getByTestId("menu-item-select")).toBeTruthy();
      expect(screen.getByTestId("menu-context")).toBeTruthy();

      // and clean
      component.unmount();
    });

    it("click on select button and change url", () => {
      Router.navigate("/");

      const component = render(<MenuArchive />);

      const selectButton = screen.getByTestId("menu-item-select");
      expect(Router.state.location.search).toBe("");

      expect(selectButton).toBeTruthy();

      selectButton?.click();

      expect(Router.state.location.search).toBe("?select=");

      // and clean
      component.unmount();
    });

    it("keyDown Tab on select button but skip", () => {
      Router.navigate("/");

      const component = render(<MenuArchive />);

      const selectButton = screen.getByTestId("menu-item-select");
      expect(Router.state.location.search).toBe("");

      expect(selectButton).toBeTruthy();

      fireEvent.keyDown(selectButton, {
        key: "Tab"
      });

      expect(Router.state.location.search).toBe("");

      // and clean
      component.unmount();
    });

    it("keyDown Enter on select button and change url", () => {
      Router.navigate("/");

      const component = render(<MenuArchive />);

      const selectButton = screen.getByTestId("menu-item-select");
      expect(Router.state.location.search).toBe("");

      expect(selectButton).toBeTruthy();

      fireEvent.keyDown(selectButton, {
        key: "Enter"
      });

      expect(Router.state.location.search).toBe("?select=");

      // and clean
      component.unmount();
    });

    it("[menu archive] check if on click the hamburger opens", () => {
      Router.navigate("/");

      const component = render(<MenuArchive />);

      const hamburger = component.getByTestId("hamburger");
      expect(hamburger?.querySelector(".open")).toBeFalsy();

      act(() => {
        hamburger?.click();
      });

      expect(hamburger?.querySelector(".open")).toBeTruthy();

      component.unmount();
    });

    it("[menu archive] none selected", () => {
      Router.navigate("?select=");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      expect(screen.getByTestId("selected-0")).toBeTruthy();

      component.unmount();
    });

    it("two selected", () => {
      Router.navigate("?select=test1,test2");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      expect(screen.getByTestId("selected-2")).toBeTruthy();

      component.unmount();
    });

    it("click on menu-archive-labels", () => {
      Router.navigate("?select=test1,test2");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const menuArchiveLabels = screen.getByTestId("menu-archive-labels") as HTMLElement;

      expect(menuArchiveLabels).toBeTruthy();

      act(() => {
        menuArchiveLabels.click();
      });

      expect(Router.state.location.search).toBe("?select=test1,test2&sidebar=true");

      component.unmount();
    });

    it("keyDown enter on menu-archive-labels", () => {
      Router.navigate("?select=test1,test2");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const menuArchiveLabels = screen.getByTestId("menu-archive-labels") as HTMLElement;

      expect(menuArchiveLabels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(menuArchiveLabels, {
          key: "Enter"
        });
      });

      expect(Router.state.location.search).toBe("?select=test1,test2&sidebar=true");

      component.unmount();
    });

    it("keyDown tab on menu-archive-labels so ignore", () => {
      Router.navigate("?select=test1,test2");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const menuArchiveLabels = screen.getByTestId("menu-archive-labels") as HTMLElement;

      expect(menuArchiveLabels).toBeTruthy();

      act(() => {
        fireEvent.keyDown(menuArchiveLabels, {
          key: "Tab"
        });
      });

      expect(Router.state.location.search).toBe("?select=test1,test2");

      component.unmount();
    });

    it("[archive] menu click mkdir", async () => {
      jest.spyOn(React, "useContext").mockReset();

      await Router.navigate("/");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const mkdirModalSpy = jest.spyOn(ModalArchiveMkdir, "default").mockImplementationOnce(() => {
        return <></>;
      });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const mkdir = screen.getByTestId("mkdir");

      expect(mkdir).toBeTruthy();

      console.log(mkdir.innerHTML);

      // need async
      await act(async () => {
        await mkdir?.click();
      });

      waitFor(() => expect(mkdirModalSpy).toHaveBeenCalled());

      component.unmount();
    });

    it("[archive] menu keyDown tab mkdir so skip", async () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const mkdirModalSpy = jest
        .spyOn(ModalArchiveMkdir, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const mkdir = screen.getByTestId("mkdir");

      // need async
      await fireEvent.keyDown(mkdir, {
        key: "Tab"
      });

      expect(mkdirModalSpy).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("[archive] menu keyDown enter mkdir", async () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const mkdirModalSpy = jest
        .spyOn(ModalArchiveMkdir, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const mkdir = screen.getByTestId("mkdir");

      // need async
      await fireEvent.keyDown(mkdir, {
        key: "Enter"
      });

      waitFor(() => expect(mkdirModalSpy).toHaveBeenCalled());

      component.unmount();
    });

    it("[archive] menu click rename (dir)", async () => {
      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalArchiveRename, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const rename = screen.getByTestId("rename");
      expect(rename).not.toBeNull();

      // need async
      await act(async () => {
        await rename?.click();
      });

      waitFor(() => expect(renameModalSpy).toHaveBeenCalled());

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] menu keydown enter rename (dir)", async () => {
      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalArchiveRename, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const rename = screen.getByTestId("rename");
      expect(rename).not.toBeNull();

      // need async
      await fireEvent.keyDown(rename, {
        key: "Enter"
      });

      waitFor(() => expect(renameModalSpy).toHaveBeenCalled());

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] menu keydown tab rename (dir) so skip", async () => {
      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalArchiveRename, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const rename = screen.getByTestId("rename");
      expect(rename).not.toBeNull();

      // need async
      await fireEvent.keyDown(rename, {
        key: "Tab"
      });

      expect(renameModalSpy).toHaveBeenCalledTimes(0);

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] menu click rename should call dispatch(dir)", () => {
      Router.navigate("/?f=/test");

      console.log("[archive] menu click rename should call dispatch(dir)");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const dispatch = jest.fn();
      const contextValues = { state, dispatch };

      const modalMockElement = (props: {
        dispatch: (action: { type: string; path: string }) => void;
      }): JSX.Element => {
        return (
          <button
            id="test-btn-fake"
            data-test="test-btn-fake"
            onClick={() => {
              if (props.dispatch) {
                props.dispatch({
                  type: "rename-folder",
                  path: "t"
                });
              }
            }}
          ></button>
        );
      };

      jest
        .spyOn(ModalArchiveRename, "default")
        .mockReset()
        .mockImplementationOnce(
          modalMockElement as React.FunctionComponent<IModalRenameFolderProps>
        )
        .mockImplementationOnce(
          modalMockElement as React.FunctionComponent<IModalRenameFolderProps>
        );

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const rename = screen.getByTestId("rename");
      expect(rename).not.toBeNull();

      act(() => {
        rename?.click();
      });

      console.log(component.container.innerHTML);

      const fakeButton = screen.getByTestId("test-btn-fake");
      fakeButton?.click();

      expect(dispatch).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledWith({
        type: "rename-folder",
        path: "t"
      });

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] display options (default menu)", () => {
      jest
        .spyOn(Link, "default")
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>);

      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const displayOptions = screen.getByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      act(() => {
        displayOptions?.click();
      });

      expect(renameModalSpy).toHaveBeenCalled();

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] display options keyDown (default menu)", () => {
      jest
        .spyOn(Link, "default")
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>);

      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const displayOptions = screen.getByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      act(() => {
        fireEvent.keyDown(displayOptions, { key: "Enter" });
      });

      expect(renameModalSpy).toHaveBeenCalled();

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] display options keyDown tab so ignore (default menu)", () => {
      jest
        .spyOn(Link, "default")
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>);

      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const displayOptions = screen.getByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      act(() => {
        // should ignore
        fireEvent.keyDown(displayOptions, { key: "Tab" });
      });

      expect(renameModalSpy).toHaveBeenCalledTimes(0);

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] click display options (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const displayOptions = screen.queryByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      act(() => {
        displayOptions?.click();
      });

      expect(renameModalSpy).toHaveBeenCalled();

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] keydown tab so skip display options (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const displayOptions = screen.queryByTestId("display-options") as HTMLElement;
      expect(displayOptions).not.toBeNull();

      act(() => {
        fireEvent.keyDown(displayOptions, { key: "Tab" });
      });

      expect(renameModalSpy).toHaveBeenCalledTimes(0);

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] keydown enter display options (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalDisplayOptions, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const displayOptions = screen.queryByTestId("display-options") as HTMLElement;
      expect(displayOptions).not.toBeNull();

      act(() => {
        fireEvent.keyDown(displayOptions, { key: "Enter" });
      });

      expect(renameModalSpy).toHaveBeenCalledTimes(1);

      component.unmount();

      Router.navigate("/");
    });

    it("[archive] synchronize-manually (default menu)", async () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/?f=/test");

      const state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalArchiveSynchronizeManually, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const syncManual = screen.getByTestId("synchronize-manually");
      expect(syncManual).not.toBeNull();

      // need async
      await syncManual?.click();

      expect(renameModalSpy).toHaveBeenCalled();

      component.unmount();
      Router.navigate("/");
    });

    it("[archive] synchronize-manually (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      jest
        .spyOn(Link, "default")
        .mockImplementationOnce(() => <></>)
        .mockImplementationOnce(() => <></>);

      Router.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const renameModalSpy = jest
        .spyOn(ModalArchiveSynchronizeManually, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const component = render(<MenuArchive />);

      const syncManual = screen.getByTestId("synchronize-manually");
      expect(syncManual).not.toBeNull();

      act(() => {
        syncManual?.click();
      });

      expect(renameModalSpy).toHaveBeenCalled();

      component.unmount();

      Router.navigate("/");
    });

    it("more and click on select all", () => {
      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const component = render(<MenuArchive />);

      const undoSelection = screen.queryByTestId("undo-selection");
      expect(undoSelection).not.toBeNull();

      act(() => {
        undoSelection?.click();
      });

      // you did press to de-select all
      expect(window.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more undoSelection", () => {
      jest.spyOn(React, "useContext").mockReset();

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(<MenuArchive />);

      const undoSelection = screen.queryByTestId("undo-selection");
      expect(undoSelection).not.toBeNull();

      act(() => {
        undoSelection?.click();
      });

      expect(window.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("keyboard ctrl a and command a", () => {
      const useHotkeysSpy = jest
        .spyOn(useHotKeys, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return { key: "a", ctrlKey: true };
        });

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      expect(useHotkeysSpy).toHaveBeenCalled();
      expect(useHotkeysSpy).toHaveBeenNthCalledWith(
        1,
        { ctrlKeyOrMetaKey: true, key: "a" },
        expect.anything(),
        []
      );

      component.unmount();
    });

    it("menu click MessageMoveToTrash", async () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/?select=test1.jpg");

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      const connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [{ fileName: "test.jpg", parentDirectory: "/" }] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(connectionDefault);
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<MenuArchive />);

      const trash = screen.queryByTestId("trash");
      expect(trash).not.toBeNull();

      console.log(trash?.innerHTML);

      // need to await
      await act(async () => {
        await trash?.click();
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlMoveToTrashApi(),
        "f=%2Fundefined%2Ftest1.jpg&Tags=%21delete%21&append=true&Colorclass=8&collections=true"
      );

      await act(async () => {
        await component.unmount();
      });
    });

    it("menu click export", () => {
      Router.navigate("/?select=test1.jpg");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const exportModalSpy = jest.spyOn(ModalDownload, "default").mockImplementationOnce(() => {
        return <></>;
      });

      const component = render(<MenuArchive />);

      const exportButton = screen.queryByTestId("export");
      expect(exportButton).not.toBeNull();

      act(() => {
        exportButton?.click();
      });

      expect(exportModalSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("[archive] menu click publish 1", () => {
      Router.navigate("/?select=test1.jpg");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const exportModalSpy = jest.spyOn(ModalPublish, "default").mockImplementationOnce(() => {
        return <></>;
      });

      const component = render(<MenuArchive />);

      const publish = screen.queryByTestId("publish");
      expect(publish).not.toBeNull();

      act(() => {
        publish?.click();
      });

      expect(exportModalSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("[archive] menu click publish 2", () => {
      Router.navigate("/?select=test1.jpg");

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      jest
        .spyOn(React, "useContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      expect(Router.state.location.search).toBe("?select=test1.jpg");

      const selectFurther = screen.queryByTestId("select-further");
      expect(selectFurther).not.toBeNull();

      act(() => {
        selectFurther?.click();
      });

      expect(Router.state.location.search).toBe("?select=test1.jpg&sidebar=false");

      component.unmount();
    });

    it("readonly - menu click mkdir", () => {
      jest.spyOn(React, "useContext").mockReset();

      Router.navigate("/");

      const state = {
        subPath: "/",
        isReadOnly: true,
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest.spyOn(ModalArchiveMkdir, "default").mockClear();

      const mkdirModalSpy = jest.spyOn(ModalArchiveMkdir, "default").mockImplementationOnce(() => {
        return <></>;
      });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const mkdir = screen.queryByTestId("mkdir");
      expect(mkdir).not.toBeNull();

      mkdir?.click();

      expect(mkdirModalSpy).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("readonly - upload", () => {
      Router.navigate("/");

      const state = {
        subPath: "/",
        isReadOnly: true,
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      const dropAreaSpy = jest.spyOn(DropArea, "default").mockImplementationOnce(() => {
        return <></>;
      });

      jest.spyOn(React, "useContext").mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      expect(dropAreaSpy).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("NavContainer MenuSearchBar callback does change state [MenuArchive]", () => {
      jest.spyOn(MenuSearchBar, "default").mockImplementationOnce((prop) => {
        if (prop.callback) {
          prop.callback("test");
        }
        return <>test</>;
      });
      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(<MenuArchive />);

      const navOpen = screen.queryByTestId("nav-open") as HTMLDivElement;

      expect(navOpen).toBeTruthy();

      component.unmount();
    });
  });
});
