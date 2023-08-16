import {
  act,
  createEvent,
  fireEvent,
  render,
  screen
} from "@testing-library/react";
import React from "react";
import * as AppContext from "../../../contexts/archive-context";
import { IArchive } from "../../../interfaces/IArchive";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Notification from "../../atoms/notification/notification";
import ArchiveSidebarLabelEditSearchReplace from "./archive-sidebar-label-edit-search-replace";
describe("ArchiveSidebarLabelEditAddOverwrite", () => {
  it("renders", () => {
    render(<ArchiveSidebarLabelEditSearchReplace />);
  });

  it("isReadOnly: true", () => {
    const mainElement = render(<ArchiveSidebarLabelEditSearchReplace />);

    const formControl = screen.queryAllByTestId("form-control");

    // there are 3 classes [title,info,description]
    formControl.forEach((element) => {
      const disabled = element.classList;
      expect(disabled).toContain("disabled");
    });

    mainElement.unmount();
  });

  describe("with context", () => {
    let useContextSpy: jest.SpyInstance;

    let dispatchedValues: any[] = [];

    beforeEach(() => {
      // is used in multiple ways
      // use this: ==> import * as AppContext from '../contexts/archive-context';
      useContextSpy = jest
        .spyOn(React, "useContext")
        .mockImplementation(() => contextValues);

      // clean array
      dispatchedValues = [];

      const contextValues = {
        state: {
          isReadOnly: false,
          fileIndexItems: [
            {
              fileName: "test.jpg",
              parentDirectory: "/"
            },
            {
              fileName: "test1.jpg",
              parentDirectory: "/"
            }
          ]
        } as IArchive,
        dispatch: (value: any) => {
          dispatchedValues.push(value);
        }
      } as AppContext.IArchiveContext;

      jest.mock("@reach/router", () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn()
      }));

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test.jpg");
      });
    });

    afterEach(() => {
      // and clean your room afterwards
      useContextSpy.mockClear();
    });

    it("isReadOnly: false (so contentEditable is true)", () => {
      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen.queryAllByTestId("form-control");

      // if there is no contentEditable it should fail
      expect(formControls.length).toBeGreaterThanOrEqual(3);

      // there are 3 classes [title,info,description]
      formControls.forEach((element) => {
        const contentEditable = element.getAttribute("contentEditable");
        expect(contentEditable).toBeTruthy();
      });

      act(() => {
        component.unmount();
      });
    });

    it("click overwrite and generic fail", async () => {
      // reject! ?>
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.reject();

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <></>);

      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      // need to await here
      const add = screen.queryByTestId("replace-button") as HTMLElement;
      await act(async () => {
        await add.click();
      });

      expect(notificationSpy).toBeCalled();

      act(() => {
        component.unmount();
      });
      jest.spyOn(Notification, "default").mockRestore();
    });

    it("click overwrite > generic fail > remove message retry when success", async () => {
      const connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [] as any[]
      };

      const mockIConnectionDefaultReject: Promise<IConnectionDefault> =
        Promise.reject();

      const mockIConnectionDefaultResolve: Promise<IConnectionDefault> =
        Promise.resolve(connectionDefault);

      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefaultReject);

      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      // need to await here
      const add = screen.queryByTestId("replace-button") as HTMLElement;
      await act(async () => {
        await add.click();
      });

      jest.spyOn(FetchPost, "default").mockRestore();
      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefaultResolve);

      // force update to show message
      let notification = screen.queryByTestId(
        "notification-content"
      ) as HTMLElement;

      expect(notification).toBeTruthy();

      // second time; now it removes the error message from the component
      // need to await here
      await act(async () => {
        await add.click();
      });

      // force update to show message
      notification = screen.queryByTestId(
        "notification-content"
      ) as HTMLElement;
      expect(notification).toBeFalsy();

      act(() => {
        component.unmount();
      });
      jest.spyOn(Notification, "default").mockRestore();
    });

    it("[replace] Should change value when onChange was called", () => {
      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      const add = screen.queryByTestId("replace-button") as HTMLElement;

      const className = add.className;
      expect(className).toBe("btn btn--default");

      act(() => {
        component.unmount();
      });
    });

    it("click replace", async () => {
      const connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [
          {
            fileName: "test.jpg",
            parentDirectory: "/",
            tags: "test1, test2",
            status: IExifStatus.Ok
          }
        ] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(connectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      const add = screen.queryByTestId("replace-button") as HTMLElement;

      expect(add).toBeTruthy();

      // need to await to contain dispatchedValues
      await act(async () => {
        await add.click();
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "f=%2Ftest.jpg&collections=true&fieldName=tags&search=a&replace="
      );

      expect(dispatchedValues).toStrictEqual([
        {
          type: "update",
          fileName: "test.jpg",
          parentDirectory: "/",
          tags: "test1, test2",
          select: ["test.jpg"],
          status: IExifStatus.Ok
        }
      ]);

      act(() => {
        component.unmount();
      });
    });

    it("click update | read only", async () => {
      jest.spyOn(FetchPost, "default").mockReset();

      const connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [
          {
            fileName: "test.jpg",
            parentDirectory: "/",
            tags: "test1, readonly",
            status: IExifStatus.ReadOnly
          }
        ] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(connectionDefault);
      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      const add = screen.queryByTestId("replace-button") as HTMLElement;
      expect(add).toBeTruthy();

      // need to await to contain dispatchedValues
      await act(async () => {
        await add.click();
      });

      const notification = screen.queryByTestId(
        "notification-content"
      ) as HTMLElement;
      expect(notification).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("click append multiple", async () => {
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test.jpg,test1.jpg,notfound.jpg");
      });

      jest.spyOn(FetchPost, "default").mockReset();

      const connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [
          {
            fileName: "test.jpg",
            parentDirectory: "/",
            tags: "test1, test2",
            status: IExifStatus.Ok
          },
          {
            fileName: "test1.jpg",
            parentDirectory: "/",
            tags: "test, test2",
            status: IExifStatus.Ok
          }
        ] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(connectionDefault);
      const spy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<ArchiveSidebarLabelEditSearchReplace />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "tags");
      const tags = formControls as HTMLElement[][0];
      expect(tags).not.toBe(undefined);

      // update component + now press a key
      tags.textContent = "a";
      const inputEvent = createEvent.input(tags, { key: "a" });
      fireEvent(tags, inputEvent);

      const add = screen.queryByTestId("replace-button") as HTMLElement;
      expect(add).toBeTruthy();

      // need to await to contain dispatchedValues
      await act(async () => {
        await add.click();
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith(
        new UrlQuery().UrlReplaceApi(),
        "f=%2Ftest.jpg%3B%2Ftest1.jpg&collections=true&fieldName=tags&search=a&replace="
      );

      expect(dispatchedValues).toStrictEqual([
        {
          type: "update",
          fileName: "test.jpg",
          parentDirectory: "/",
          tags: "test1, test2",
          select: ["test.jpg"],
          status: IExifStatus.Ok
        },
        {
          type: "update",
          fileName: "test1.jpg",
          parentDirectory: "/",
          tags: "test, test2",
          select: ["test1.jpg"],
          status: IExifStatus.Ok
        }
      ]);

      component.unmount();
    });
  });
});
