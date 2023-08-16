import { render, screen } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useFetch from "../../../hooks/use-fetch";
import * as useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IArchive } from "../../../interfaces/IArchive";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import MenuTrash from "./menu-trash";
;

describe("MenuTrash", () => {
  it("renders", () => {
    render(
      <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={jest.fn()} />
    );
  });

  describe("with Context", () => {
    let contextValues: any;

    beforeEach(() => {
      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Deleted,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;

      contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());
    });

    it("open hamburger menu (MenuTrash)", () => {
      const component = render(
        <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={jest.fn()} />
      );

      let hamburger = screen.queryByTestId("hamburger") as HTMLDivElement;
      let hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger");

      act(() => {
        hamburger.click();
      });

      hamburger = screen.queryByTestId("hamburger") as HTMLDivElement;
      hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger open");

      component.unmount();
    });

    it("select is not disabled", () => {
      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = screen.queryByTestId(
        "menu-trash-item-select"
      ) as HTMLDivElement;
      expect(menuTrashItemSelect).toBeTruthy();

      component.unmount();
    });

    it("select toggle", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        window.location.replace("/");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = screen.queryByTestId(
        "menu-trash-item-select"
      ) as HTMLDivElement;
      expect(menuTrashItemSelect).toBeTruthy();

      menuTrashItemSelect.click();

      expect(globalHistory.location.search).toBe("?select=");
      component.unmount();
    });

    it("more select all", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/?select=");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuContext = screen.queryByTestId(
        "menu-context"
      ) as HTMLInputElement;
      const menuContextParent = menuContext.parentElement as HTMLInputElement;
      expect(menuContextParent.classList).not.toContain("disabled");

      screen.queryByTestId("select-all")?.click();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/");
        component.unmount();
      });
    });

    it("more undoSelection", () => {
      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuContext = screen.queryByTestId(
        "menu-context"
      ) as HTMLInputElement;
      const menuContextParent = menuContext.parentElement as HTMLInputElement;
      expect(menuContextParent.classList).not.toContain("disabled");

      screen.queryByTestId("undo-selection")?.click();

      expect(globalHistory.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/");
        component.unmount();
      });
    });

    it("more force delete, expect modal", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      const modalSpy = jest
        .spyOn(Modal, "default")
        .mockImplementationOnce(({ children }) => {
          return <>{children}</>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/");
        component.unmount();
      });
    });

    it("more force delete, expect modal 2", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      const modalSpy = jest
        .spyOn(Modal, "default")
        .mockImplementationOnce(({ children }) => {
          return <span id="test">{children}</span>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/");
        component.unmount();
      });
    });

    it("more restore-from-trash", async () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        });

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("restore-from-trash");

      // // need to await here
      await act(async () => {
        await item?.click();
      });

      expect(globalHistory.location.search).toBe("?select=");

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "fieldName=tags&search=%21delete%21&f=%2Fundefined%2Ftest1.jpg"
      );

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        window.location.replace("/");
        component.unmount();
      });
    });

    it("keyboard ctrl a and command a", () => {
      jest.spyOn(React, "useContext").mockRestore();

      const useHotkeysSpy = jest
        .spyOn(useHotKeys, "default")
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
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      expect(useHotkeysSpy).toBeCalled();
      expect(useHotkeysSpy).toBeCalledTimes(1);

      jest.spyOn(React, "useContext").mockRestore();
      component.unmount();
    });
  });
});
