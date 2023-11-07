import { fireEvent, render, screen } from "@testing-library/react";
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
import { Router } from "../../../router-app/router-app";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import * as MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import * as NavContainer from "../nav-container/nav-container";
import MenuTrash from "./menu-trash";

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

    it("select toggle click", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        Router.navigate("/");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = screen.queryByTestId(
        "menu-trash-item-select"
      ) as HTMLDivElement;
      expect(menuTrashItemSelect).toBeTruthy();

      menuTrashItemSelect.click();

      expect(Router.state.location.search).toBe("?select=");
      component.unmount();
    });

    it("select toggle keyDown enter continue", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        Router.navigate("/");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = screen.queryByTestId(
        "menu-trash-item-select"
      ) as HTMLDivElement;
      expect(menuTrashItemSelect).toBeTruthy();

      fireEvent.keyDown(menuTrashItemSelect, {
        key: "Enter"
      });

      expect(Router.state.location.search).toBe("?select=");
      component.unmount();
    });

    it("select toggle keyDown tab skip", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return contextValues;
      });

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        Router.navigate("/");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = screen.queryByTestId(
        "menu-trash-item-select"
      ) as HTMLDivElement;
      expect(menuTrashItemSelect).toBeTruthy();

      fireEvent.keyDown(menuTrashItemSelect, {
        key: "Tab"
      });

      expect(Router.state.location.search).toBe("");

      component.unmount();
    });

    it("more select all", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=");
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

      expect(Router.state.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
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
        Router.navigate("/?select=test1.jpg");
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

      expect(Router.state.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more force delete, expect modal 1", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      const modalSpy = jest
        .spyOn(Modal, "default")
        .mockReset()
        .mockImplementationOnce(({ children }) => {
          return <>{children}</>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(window.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more keyDown delete tab so skip", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      const modalSpy = jest
        .spyOn(Modal, "default")
        .mockReset()
        .mockImplementationOnce(({ children }) => {
          return <>{children}</>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete") as HTMLElement;

      act(() => {
        fireEvent.keyDown(item, { key: "Tab" });
      });

      expect(modalSpy).toBeCalledTimes(0);

      expect(window.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more keyDown delete enter", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      const modalSpy = jest
        .spyOn(Modal, "default")
        .mockReset()
        .mockImplementationOnce(({ children }) => {
          return <>{children}</>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete") as HTMLElement;

      act(() => {
        fireEvent.keyDown(item, { key: "Enter" });
      });

      expect(modalSpy).toBeCalledTimes(1);

      expect(window.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
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
        .mockReset()
        .mockImplementationOnce(({ children }) => {
          return <span id="test">{children}</span>;
        });
      Router.navigate("/?select=test1.jpg");

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(Router.state.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more restore-from-trash", async () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("restore-from-trash");

      // // need to await here
      await act(async () => {
        await item?.click();
      });

      expect(Router.state.location.search).toBe("?select=");

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "fieldName=tags&search=%21delete%21&f=%2Fundefined%2Ftest1.jpg"
      );

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("more restore-from-trash keyboardDown", async () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test1.jpg");
      });

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const item = screen.queryByTestId("restore-from-trash") as HTMLElement;

      // // need to await here
      await act(async () => {
        fireEvent.keyDown(item, {
          key: "Enter"
        });
      });

      expect(Router.state.location.search).toBe("?select=");

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "fieldName=tags&search=%21delete%21&f=%2Fundefined%2Ftest1.jpg"
      );

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/");
        component.unmount();
      });
    });

    it("keyboard ctrl a and command a", () => {
      jest.spyOn(React, "useContext").mockRestore();
      jest.spyOn(NavContainer, "default").mockImplementationOnce(() => <></>);

      const useHotkeysSpy = jest
        .spyOn(useHotKeys, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return { key: "a", ctrlKey: true };
        })
        .mockImplementationOnce(() => {
          return {};
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
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      expect(useHotkeysSpy).toBeCalled();
      expect(useHotkeysSpy).toHaveBeenNthCalledWith(
        1,
        {
          ctrlKeyOrMetaKey: true,
          key: "a"
        },
        expect.any(Function),
        []
      );

      jest.spyOn(React, "useContext").mockRestore();
      component.unmount();
    });

    it("NavContainer MenuSearchBar callback does change state [MenuTrash]", () => {
      jest.spyOn(MenuSearchBar, "default").mockImplementationOnce((prop) => {
        if (prop.callback) {
          prop.callback("test");
        }
        return <>test</>;
      });

      const component = render(
        <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={jest.fn()} />
      );

      const navOpen = screen.queryByTestId("nav-open") as HTMLDivElement;

      expect(navOpen).toBeTruthy();

      component.unmount();
    });
  });
});
