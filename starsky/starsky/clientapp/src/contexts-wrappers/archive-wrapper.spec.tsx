import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import * as Archive from "../containers/archive";
import * as Login from "../containers/login";
import * as Search from "../containers/search";
import * as Trash from "../containers/trash";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { mountReactHook } from "../hooks/___tests___/test-hook";
import { newIArchive } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import {
  IFileIndexItem,
  newIFileIndexItem
} from "../interfaces/IFileIndexItem";
import ArchiveContextWrapper, {
  ArchiveEventListenerUseEffect,
  filterArchiveFromEvent
} from "./archive-wrapper";

describe("ArchiveContextWrapper", () => {
  it("renders", () => {
    shallow(<ArchiveContextWrapper {...newIArchive()} />);
  });

  describe("with mount", () => {
    it("check if archive is mounted", () => {
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Archive
      } as IArchiveProps;
      var archive = jest
        .spyOn(Archive, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = mount(
        <ArchiveContextWrapper {...args}></ArchiveContextWrapper>
      );
      expect(archive).toBeCalled();
      component.unmount();
    });

    it("check if search is mounted", () => {
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search,
        searchQuery: ""
      } as IArchiveProps;
      var search = jest.spyOn(Search, "default").mockImplementationOnce(() => {
        return <></>;
      });

      // for loading
      jest.spyOn(Archive, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);

      const component = mount(
        <ArchiveContextWrapper {...args}></ArchiveContextWrapper>
      );
      expect(search).toBeCalled();
      component.unmount();
    });

    it("check if Unauthorized/Login is mounted", () => {
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Unauthorized
      } as IArchiveProps;
      var login = jest.spyOn(Login, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = mount(<ArchiveContextWrapper {...args} />);
      expect(login).toBeCalled();
      component.unmount();
    });

    it("check if Trash is mounted", () => {
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Trash
      } as IArchiveProps;
      var login = jest.spyOn(Trash, "default").mockImplementationOnce(() => {
        return <></>;
      });

      args.fileIndexItems.push({} as IFileIndexItem);
      const component = mount(<ArchiveContextWrapper {...args} />);
      expect(login).toBeCalled();
      component.unmount();
    });
  });

  describe("no context", () => {
    it("No context if used", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return { state: null, dispatch: jest.fn() };
      });
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      var component = mount(
        <ArchiveContextWrapper {...args}></ArchiveContextWrapper>
      );

      expect(component.text()).toBe("(ArchiveWrapper) = no state");
      component.unmount();
    });

    it("No fileIndexItems", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return { state: { fileIndexItems: undefined }, dispatch: jest.fn() };
      });
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      var component = mount(
        <ArchiveContextWrapper {...args}></ArchiveContextWrapper>
      );

      expect(component.text()).toBe("");
      component.unmount();
    });

    it("No pageType", () => {
      jest.spyOn(React, "useContext").mockImplementationOnce(() => {
        return {
          state: { fileIndexItems: [], pageType: undefined },
          dispatch: jest.fn()
        };
      });
      var args = {
        ...newIArchive(),
        fileIndexItems: [],
        pageType: PageType.Search
      } as IArchiveProps;
      var component = mount(
        <ArchiveContextWrapper {...args}></ArchiveContextWrapper>
      );

      expect(component.text()).toBe("");
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
      // @ts-ignore
      delete window.location;
      // @ts-ignore
      window.location = {
        search: "/?f=/"
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("Check if event is received", () => {
      var dispatch = (e: any) => {
        // should ignore the first one
        expect(e).toStrictEqual({
          add: [
            {
              colorclass: undefined,
              description: "",
              fileName: "test",
              filePath: "/test.jpg",
              parentDirectory: "/",
              tags: "",
              title: ""
            }
          ],
          type: "add"
        });
      };

      var result = mountReactHook(ArchiveEventListenerUseEffect, [dispatch]);
      var detail = [
        {
          colorclass: undefined,
          ...newIFileIndexItem(),
          filePath: "/test.jpg",
          fileName: "test",
          parentDirectory: "/"
        }
      ];
      var event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      var element = (result.componentMount as any) as ReactWrapper;
      element.unmount();
    });

    it("When outside current directory it should be ignored 2", () => {
      var dispatch = (e: any) => {
        // should ignore the first one
        expect(e).toStrictEqual({
          add: [],
          type: "add"
        });
      };

      var result = mountReactHook(ArchiveEventListenerUseEffect, [dispatch]);
      var detail = [
        {
          colorclass: undefined,
          ...newIFileIndexItem(),
          filePath: "/test.jpg",
          fileName: "test",
          parentDirectory: "__something__different"
        }
      ];
      var event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      var element = (result.componentMount as any) as ReactWrapper;
      element.unmount();
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

    it("should include parent folder", () => {
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
  });
});
