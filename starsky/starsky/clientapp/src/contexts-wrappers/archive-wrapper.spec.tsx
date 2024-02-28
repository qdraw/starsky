import { render } from "@testing-library/react";
import React from "react";
import * as Archive from "../containers/archive/archive";
import * as Login from "../containers/login";
import * as Search from "../containers/search";
import * as Trash from "../containers/trash";
import { mountReactHook } from "../hooks/___tests___/test-hook";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { newIArchive } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { IFileIndexItem, newIFileIndexItem } from "../interfaces/IFileIndexItem";
import ArchiveContextWrapper, {
  ArchiveEventListenerUseEffect,
  dispatchEmptyFolder,
  filterArchiveFromEvent
} from "./archive-wrapper";

describe("ArchiveContextWrapper", () => {
  it("renders", () => {
    render(<ArchiveContextWrapper {...newIArchive()} />);
  });

  describe("with mount", () => {
    it("check if archive is mounted", () => {
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Archive
      } as IArchiveProps;
      const archive = jest.spyOn(Archive, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = render(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
      expect(archive).toHaveBeenCalled();
      component.unmount();
    });

    it("check if search is mounted", () => {
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search,
        searchQuery: ""
      } as IArchiveProps;
      const search = jest.spyOn(Search, "default").mockImplementationOnce(() => {
        return <></>;
      });

      // for loading
      jest.spyOn(Archive, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);

      const component = render(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
      expect(search).toHaveBeenCalled();
      component.unmount();
    });

    it("check if Unauthorized/Login is mounted", () => {
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Unauthorized
      } as IArchiveProps;
      const login = jest.spyOn(Login, "Login").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = render(<ArchiveContextWrapper {...args} />);
      expect(login).toHaveBeenCalled();
      component.unmount();
    });

    it("check if Trash is mounted", () => {
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Trash
      } as IArchiveProps;
      const login = jest.spyOn(Trash, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = render(<ArchiveContextWrapper {...args} />);
      expect(login).toHaveBeenCalled();
      component.unmount();
    });
  });

  describe("no context", () => {
    it("No context if used", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return { state: null, dispatch: jest.fn() };
      });
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      const component = render(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(component.container.innerHTML).toBe("(ArchiveWrapper) = no state");
      component.unmount();
    });

    it("No fileIndexItems", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return { state: { fileIndexItems: undefined }, dispatch: jest.fn() };
      });
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      const component = render(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(component.container.innerHTML).toBe("");
      component.unmount();
    });

    it("No pageType", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return {
          state: { fileIndexItems: [], pageType: undefined },
          dispatch: jest.fn()
        };
      });
      const args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      const component = render(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(component.container.innerHTML).toBe("");
      component.unmount();
    });
  });

  describe("ArchiveEventListenerUseEffect / updateArchiveFromEvent", () => {
    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      delete window.location;
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      window.location = {
        search: "/?f=/"
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("Check if event is received", () => {
      const dispatch = (e: any) => {
        // should ignore the first one
        expect(e).toStrictEqual({
          add: [
            {
              colorclass: undefined,
              description: "",
              fileName: "test",
              filePath: "/test.jpg",
              parentDirectory: "/",
              status: IExifStatus.Ok,
              tags: "",
              title: ""
            }
          ],
          type: "add"
        });
      };

      const result = mountReactHook(ArchiveEventListenerUseEffect, [dispatch]);
      const detail = {
        data: [
          {
            colorclass: undefined,
            ...newIFileIndexItem(),
            filePath: "/test.jpg",
            fileName: "test",
            parentDirectory: "/",
            status: IExifStatus.Ok
          }
        ]
      };
      const event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      result.componentMount.unmount();
    });

    it("When outside current directory it should be ignored 2", () => {
      const dispatch = (e: any) => {
        // should ignore the first one
        expect(e).toStrictEqual({
          add: [],
          type: "add"
        });
      };

      const result = mountReactHook(ArchiveEventListenerUseEffect, [dispatch]);
      const detail = {
        data: [
          {
            colorclass: undefined,
            ...newIFileIndexItem(),
            filePath: "/test.jpg",
            fileName: "test",
            parentDirectory: "__something__different"
          }
        ]
      };
      const event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      result.componentMount.unmount();
    });
  });

  describe("dispatchEmptyFolder", () => {
    it("should dispatch when source folder is not found", () => {
      const list = [
        {
          filePath: "/test",
          parentDirectory: "/",
          status: IExifStatus.NotFoundSourceMissing
        }
      ] as IFileIndexItem[];

      const dispatch = jest.fn();
      dispatchEmptyFolder(list, "/test", dispatch);
      expect(dispatch).toHaveBeenCalled();
    });

    it("should not dispatch when source folder is oke", () => {
      const list = [
        {
          filePath: "/test",
          parentDirectory: "/",
          status: IExifStatus.Ok
        }
      ] as IFileIndexItem[];

      const dispatch = jest.fn();
      dispatchEmptyFolder(list, "/test", dispatch);
      expect(dispatch).toHaveBeenCalledTimes(0);
    });

    it("should not dispatch when location is diff", () => {
      const list = [
        {
          filePath: "/test/image.jpg",
          parentDirectory: "/test",
          status: IExifStatus.Ok
        }
      ] as IFileIndexItem[];

      const dispatch = jest.fn();
      dispatchEmptyFolder(list, "/test", dispatch);
      expect(dispatch).toHaveBeenCalledTimes(0);
    });
  });

  describe("updateArchiveFromEvent", () => {
    it("should ignore child folder", () => {
      const list = [
        {
          filePath: "/test.jpg",
          parentDirectory: "/"
        },
        {
          filePath: "/child/test.jpg",
          parentDirectory: "/child"
        }
      ] as IFileIndexItem[];
      const result = filterArchiveFromEvent(list, "/");
      expect(result.length).toBe(1);
    });

    it("should not include parent folder", () => {
      const list = [
        {
          filePath: "/",
          parentDirectory: "/"
        },
        {
          filePath: "/test.jpg",
          parentDirectory: "/"
        }
      ] as IFileIndexItem[];
      const result = filterArchiveFromEvent(list, "/");
      expect(result.length).toBe(2);
    });

    it("should not include parent folder [undefined]", () => {
      const list = [
        {
          filePath: "/",
          parentDirectory: "/"
        },
        {
          filePath: "/test.jpg",
          parentDirectory: "/"
        }
      ] as IFileIndexItem[];
      const result = filterArchiveFromEvent(list, undefined);
      expect(result.length).toBe(2);
    });
  });
});
