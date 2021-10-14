import { globalHistory } from "@reach/router";
import { act, render } from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IArchive } from "../../../interfaces/IArchive";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as DropArea from "../../atoms/drop-area/drop-area";
import * as ModalArchiveMkdir from "../modal-archive-mkdir/modal-archive-mkdir";
import * as ModalArchiveRename from "../modal-archive-rename/modal-archive-rename";
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
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        });

      jest
        .spyOn(window, "scrollTo")
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {});
    });

    it("default menu", () => {
      globalHistory.navigate("/");

      var component = render(<MenuArchive />);

      expect(component.queryByTestId("hamburger")).toBeTruthy();
      expect(component.queryByTestId("menu-item-select")).toBeTruthy();
      expect(component.queryByTestId("menu-context")).toBeTruthy();

      // and clean
      component.unmount();
    });

    it("check if on click the hamburger opens", () => {
      var component = render(<MenuArchive />);

      const hamburger = component.queryByTestId("hamburger");
      expect(hamburger?.querySelector(".open")).toBeFalsy();

      hamburger?.click();
      expect(hamburger?.querySelector(".open")).toBeTruthy();

      component.unmount();
    });

    it("none selected", () => {
      globalHistory.navigate("?select=");

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var component = render(<MenuArchive>t</MenuArchive>);

      expect(component.queryByTestId("selected-0")).toBeTruthy();

      component.unmount();
    });

    it("two selected", () => {
      globalHistory.navigate("?select=test1,test2");

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var component = render(<MenuArchive>t</MenuArchive>);

      expect(component.queryByTestId("selected-2")).toBeTruthy();

      component.unmount();
    });

    it("menu click mkdir", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var mkdirModalSpy = jest
        .spyOn(ModalArchiveMkdir, "default")
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

      var component = render(<MenuArchive>t</MenuArchive>);

      const mkdir = component.queryByTestId("mkdir");

      mkdir?.click();

      expect(mkdirModalSpy).toBeCalled();

      component.unmount();
    });

    it("menu click rename (dir)", () => {
      globalHistory.navigate("/?f=/test");

      var state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var renameModalSpy = jest
        .spyOn(ModalArchiveRename, "default")
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

      var component = render(<MenuArchive>t</MenuArchive>);

      const rename = component.queryByTestId("rename");
      expect(rename).not.toBeNull();

      rename?.click();

      expect(renameModalSpy).toBeCalled();

      component.unmount();

      globalHistory.navigate("/");
    });

    it("menu click rename should call dispatch(dir)", () => {
      globalHistory.navigate("/?f=/test");

      var state = {
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
      var contextValues = { state, dispatch };

      jest
        .spyOn(ModalArchiveRename, "default")
        .mockImplementationOnce((props) => {
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
        });

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      var component = render(<MenuArchive>t</MenuArchive>);

      const rename = component.queryByTestId("rename");
      expect(rename).not.toBeNull();

      rename?.click();

      const fakeButton = component.queryByTestId("test-btn-fake");
      fakeButton?.click();

      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith({
        type: "rename-folder",
        path: "t"
      });

      component.unmount();

      globalHistory.navigate("/");
    });

    it("display options (default menu)", () => {
      jest.spyOn(React, "useContext").mockRestore();

      globalHistory.navigate("/?f=/test");

      var state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var renameModalSpy = jest
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
        });

      var component = render(<MenuArchive />);

      const displayOptions = component.queryByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      displayOptions?.click();

      expect(renameModalSpy).toBeCalled();

      component.unmount();

      globalHistory.navigate("/");
    });

    it("display options (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      var state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var renameModalSpy = jest
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

      var component = render(<MenuArchive />);

      const displayOptions = component.queryByTestId("display-options");
      expect(displayOptions).not.toBeNull();

      displayOptions?.click();

      expect(renameModalSpy).toBeCalled();

      component.unmount();

      globalHistory.navigate("/");
    });

    it("synchronize-manually (default menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?f=/test");

      var state = {
        subPath: "/test",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var renameModalSpy = jest
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

      var component = render(<MenuArchive />);

      const syncManual = component.queryByTestId("synchronize-manually");
      expect(syncManual).not.toBeNull();

      syncManual?.click();

      expect(renameModalSpy).toBeCalled();

      component.unmount();
      globalHistory.navigate("/");
    });

    it("synchronize-manually (select menu)", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?f=/trashed&select=test1.jpg");

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      var state = {
        subPath: "/trashed",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      var renameModalSpy = jest
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

      var component = render(<MenuArchive />);

      const syncManual = component.queryByTestId("synchronize-manually");
      expect(syncManual).not.toBeNull();

      syncManual?.click();

      expect(renameModalSpy).toBeCalled();

      component.unmount();

      globalHistory.navigate("/");
    });

    it("more and click on select all", () => {
      jest.spyOn(React, "useContext").mockReset();

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)

        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        });

      var component = render(<MenuArchive />);

      const undoSelection = component.queryByTestId("undo-selection");
      expect(undoSelection).not.toBeNull();

      undoSelection?.click();

      // you did press to de-select all
      expect(globalHistory.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("more undoSelection", () => {
      jest.spyOn(React, "useContext").mockReset();

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

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
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = render(<MenuArchive />);

      const undoSelection = component.queryByTestId("undo-selection");
      expect(undoSelection).not.toBeNull();

      undoSelection?.click();

      expect(globalHistory.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("keyboard ctrl a and command a", () => {
      jest.spyOn(React, "useContext").mockReset();

      const useHotkeysSpy = jest
        .spyOn(useHotKeys, "default")
        .mockImplementationOnce(() => {
          return { key: "a", ctrlKey: true };
        });

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      var component = render(<MenuArchive />);

      expect(useHotkeysSpy).toBeCalled();
      expect(useHotkeysSpy).toBeCalledTimes(1);

      component.unmount();
    });

    it("menu click MessageMoveToTrash", async () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?select=test1.jpg");

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

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
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [
          { fileName: "test.jpg", parentDirectory: "/" }
        ] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        connectionDefault
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      var component = render(<MenuArchive />);

      // var item: ReactWrapper;
      // await act(async () => {
      //   item = await component.find('[data-test="trash"]');
      // });

      // await act(async () => {
      //   await item.simulate("click");
      // });

      const trash = component.queryByTestId("trash");
      expect(trash).not.toBeNull();

      trash?.click();

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        "f=%2Fundefined%2Ftest1.jpg&Tags=%21delete%21&append=true&Colorclass=8&collections=true"
      );

      await act(async () => {
        await component.unmount();
      });
    });

    it("menu click export", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?select=test1.jpg");

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault());

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

      var exportModalSpy = jest
        .spyOn(ModalDownload, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = render(<MenuArchive />);

      const exportButton = component.queryByTestId("export");
      expect(exportButton).not.toBeNull();

      exportButton?.click();

      expect(exportModalSpy).toBeCalled();

      component.unmount();
    });

    it("menu click publish", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/?select=test1.jpg");

      var state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault());

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

      var exportModalSpy = jest
        .spyOn(ModalPublish, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      var component = render(<MenuArchive />);

      const publish = component.queryByTestId("publish");
      expect(publish).not.toBeNull();

      publish?.click();

      expect(exportModalSpy).toBeCalled();

      component.unmount();
    });

    it("readonly - menu click mkdir", () => {
      jest.spyOn(React, "useContext").mockReset();

      globalHistory.navigate("/");

      var state = {
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
      var contextValues = { state, dispatch: jest.fn() };

      jest.spyOn(ModalArchiveMkdir, "default").mockClear();

      var mkdirModalSpy = jest
        .spyOn(ModalArchiveMkdir, "default")
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

      var component = render(<MenuArchive />);

      const mkdir = component.queryByTestId("mkdir");
      expect(mkdir).not.toBeNull();

      mkdir?.click();

      expect(mkdirModalSpy).toBeCalledTimes(0);

      component.unmount();
    });

    it("readonly - upload", () => {
      globalHistory.navigate("/");

      var state = {
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
      var contextValues = { state, dispatch: jest.fn() };

      var dropAreaSpy = jest
        .spyOn(DropArea, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      var component = render(<MenuArchive />);

      expect(dropAreaSpy).toBeCalledTimes(0);

      component.unmount();
    });
  });
});
