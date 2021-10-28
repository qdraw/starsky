import { globalHistory } from "@reach/router";
import { act, render } from "@testing-library/react";
import React from "react";
import * as AppContext from "../../../contexts/archive-context";
import { newIArchive } from "../../../interfaces/IArchive";
import { PageType } from "../../../interfaces/IDetailView";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import * as ColorClassSelect from "../color-class-select/color-class-select";
import ArchiveSidebarColorClass from "./archive-sidebar-color-class";

describe("ArchiveSidebarColorClass", () => {
  it("renders", () => {
    render(
      <ArchiveSidebarColorClass
        pageType={PageType.Archive}
        fileIndexItems={newIFileIndexItemArray()}
        isReadOnly={false}
      />
    );
  });

  describe("mount object (mount= select is child element)", () => {
    function wrapperHelper() {
      return render(
        <ArchiveSidebarColorClass
          pageType={PageType.Archive}
          fileIndexItems={newIFileIndexItemArray()}
          isReadOnly={false}
        />
      );
    }
    it("colorclass--select class exist", () => {
      expect(wrapperHelper().container.innerHTML).toContain(
        "colorclass--select"
      );
    });

    it("not disabled", () => {
      expect(wrapperHelper().container.innerHTML).not.toContain(" disabled");
    });

    it("Fire event when clicked", () => {
      // Warning: An update to null inside a test was not wrapped in act(...)

      // is used in multiple ways
      // use this: ==> import * as AppContext from '../contexts/archive-context';
      var useContextSpy = jest
        .spyOn(React, "useContext")
        .mockImplementation(() => contextValues);

      var dispatch = jest.fn();
      const contextValues = {
        state: newIArchive(),
        dispatch
      } as AppContext.IArchiveContext;

      jest.mock("@reach/router", () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn()
      }));

      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?select=test.jpg");

      var isCalled = false;
      jest
        .spyOn(ColorClassSelect, "default")
        .mockImplementationOnce(() => {
          return <div data-test="color-class-select-0"></div>;
        })
        .mockImplementationOnce((data) => {
          return (
            <button
              data-test="color-class-select-0"
              onClick={() => {
                data.onToggle(1);
                isCalled = true;
              }}
              className="colorclass--1"
            ></button>
          );
        });

      const element1 = render(
        <ArchiveSidebarColorClass
          pageType={PageType.Archive}
          isReadOnly={false}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      act(() => {
        element1.queryByTestId("color-class-select-0")?.click();
      });

      expect(isCalled).toBeTruthy();
      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith({
        colorclass: 1,
        select: ["test.jpg"],
        type: "update"
      });

      useContextSpy.mockClear();
    });
  });
});
