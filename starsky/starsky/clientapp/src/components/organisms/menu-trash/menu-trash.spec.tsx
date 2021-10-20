import { globalHistory } from "@reach/router";
import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useFetch from "../../../hooks/use-fetch";
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

describe("MenuTrash", () => {
  it("renders", () => {
    render(
      <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={jest.fn()} />
    );
  });

  describe("with Context", () => {
    let contextValues: any;

    beforeEach(() => {
      var state = {
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
    });

    it("open hamburger menu (MenuTrash)", () => {
      var component = render(
        <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={jest.fn()} />
      );

      let hamburger = component.queryByTestId("hamburger") as HTMLDivElement;
      let hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger");

      act(() => {
        hamburger.click();
      });

      hamburger = component.queryByTestId("hamburger") as HTMLDivElement;
      hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger open");

      component.unmount();
    });

    it("select is not disabled", () => {
      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = component.queryByTestId(
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
        globalHistory.navigate("/");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuTrashItemSelect = component.queryByTestId(
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
        globalHistory.navigate("/?select=");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuContext = component.queryByTestId(
        "menu-context"
      ) as HTMLInputElement;
      const menuContextParent = menuContext.parentElement as HTMLInputElement;
      expect(menuContextParent.classList).not.toContain("disabled");

      component.queryByTestId("select-all")?.click();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
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
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      const menuContext = component.queryByTestId(
        "menu-context"
      ) as HTMLInputElement;
      const menuContextParent = menuContext.parentElement as HTMLInputElement;
      expect(menuContextParent.classList).not.toContain("disabled");

      component.queryByTestId("undo-selection")?.click();

      expect(globalHistory.location.search).toBe("?select=");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("more force delete, expect modal", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      var modalSpy = jest
        .spyOn(Modal, "default")
        .mockImplementationOnce(({ children }) => {
          return <>{children}</>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      var item = component.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("more force delete, expect modal 2", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      var modalSpy = jest
        .spyOn(Modal, "default")
        .mockImplementationOnce(({ children }) => {
          return <span id="test">{children}</span>;
        });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      var item = component.queryByTestId("delete");

      act(() => {
        item?.click();
      });

      expect(modalSpy).toBeCalled();

      expect(globalHistory.location.search).toBe("?select=test1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
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
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        newIConnectionDefault()
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = render(
        <MenuTrash state={contextValues.state} dispatch={jest.fn()} />
      );

      var item = component.queryByTestId("restore-from-trash");

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
        globalHistory.navigate("/");
        component.unmount();
      });
    });
  });
});
