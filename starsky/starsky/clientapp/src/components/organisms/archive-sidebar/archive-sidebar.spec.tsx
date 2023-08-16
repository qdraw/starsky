import { act, render, screen } from "@testing-library/react";
import React, { useEffect } from "react";
import { PageType } from "../../../interfaces/IDetailView";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ArchiveSidebar from "./archive-sidebar";
;

describe("ArchiveSidebar", () => {
  it("renders", () => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

    render(
      <ArchiveSidebar
        pageType={PageType.Loading}
        subPath={"/"}
        isReadOnly={true}
        colorClassUsage={[]}
        fileIndexItems={newIFileIndexItemArray()}
      />
    );
  });

  describe("with mount", () => {
    beforeEach(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      jest.spyOn(React, "useLayoutEffect").mockImplementation(useEffect);

      window.location.replace("/?sidebar=true");
    });

    it("restore scroll after unmount", () => {
      jest.spyOn(window, "scrollTo").mockReset();
      const scrollTo = jest
        .spyOn(window, "scrollTo")
        .mockImplementationOnce(() => {});

      const component = render(
        <ArchiveSidebar
          pageType={PageType.Archive}
          subPath={"/"}
          isReadOnly={true}
          colorClassUsage={[]}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      act(() => {
        component.unmount();
      });

      expect(document.body.classList.contains("lock-screen")).toBeFalsy();
      expect(scrollTo).toBeCalled();
      expect(scrollTo).toBeCalledWith(0, -0);
    });

    it("no warning if is not read only", () => {
      const component = render(
        <ArchiveSidebar
          pageType={PageType.Archive}
          subPath={"/"}
          isReadOnly={false}
          colorClassUsage={[]}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      const element = screen.queryByTestId(
        "sidebar-selection-none"
      ) as HTMLDivElement;

      expect(element).toBeTruthy();

      component.unmount();
    });

    it("show warning if is read only", () => {
      const component = render(
        <ArchiveSidebar
          pageType={PageType.Archive}
          subPath={"/"}
          isReadOnly={true}
          colorClassUsage={[]}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      const element = screen.queryByTestId(
        "sidebar-read-only"
      ) as HTMLDivElement;

      expect(element).toBeTruthy();

      component.unmount();
    });

    it("scroll event and set body style with scroll", () => {
      render(
        <ArchiveSidebar
          pageType={PageType.Archive}
          subPath={"/"}
          isReadOnly={false}
          colorClassUsage={[]}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      Object.defineProperty(window, "scrollY", { value: 1 });
      window.dispatchEvent(new Event("scroll"));

      expect(document.body.style.top).toBe("-1px");

      // reset
      Object.defineProperty(window, "scrollY", { value: 0 });
      document.body.style.top = "";
    });

    it("scroll event and set body style no scroll", () => {
      render(
        <ArchiveSidebar
          pageType={PageType.Archive}
          subPath={"/"}
          isReadOnly={false}
          colorClassUsage={[]}
          fileIndexItems={newIFileIndexItemArray()}
        />
      );

      window.dispatchEvent(new Event("scroll"));
      expect(document.body.style.top).toBe("");
    });
  });
});
