import { render } from "@testing-library/react";
import { act } from "react";
import { MemoryRouter } from "react-router-dom";
import * as useLocation from "../../../hooks/use-location/use-location";
import { IRelativeObjects, newIRelativeObjects } from "../../../interfaces/IDetailView";
import ArchivePagination from "./archive-pagination";

describe("ArchivePagination", () => {
  it("renders new object", () => {
    const item = render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={newIRelativeObjects()} />
      </MemoryRouter>
    );
    expect(item).toBeTruthy();
  });

  const relativeObjects = {
    nextFilePath: "next",
    prevFilePath: "prev"
  } as IRelativeObjects;

  it("next page exist", () => {
    const Component = render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={relativeObjects} />
      </MemoryRouter>
    );
    const next = Component.queryByTestId("archive-pagination-next") as HTMLAnchorElement;
    expect(next.href).toBe("http://localhost/?f=next");
  });

  it("prev page exist", () => {
    const Component = render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={relativeObjects} />
      </MemoryRouter>
    );
    const next = Component.queryByTestId("archive-pagination-prev") as HTMLAnchorElement;
    expect(next.href).toBe("http://localhost/?f=prev");
  });

  describe("keyboard navigation", () => {
    const relativeObjectsWithPaths = {
      nextFilePath: "/folder/next",
      prevFilePath: "/folder/prev"
    } as IRelativeObjects;

    const relativeObjectsNullNext = {
      nextFilePath: null,
      prevFilePath: "/folder/prev"
    } as unknown as IRelativeObjects;

    const relativeObjectsNullPrev = {
      nextFilePath: "/folder/next",
      prevFilePath: null
    } as unknown as IRelativeObjects;

    it("ArrowLeft navigates to prev", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsWithPaths} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "ArrowLeft" })
        );
      });

      expect(navigateSpy).toHaveBeenCalledWith("/?f=/folder/prev", { replace: true });
      component.unmount();
    });

    it("ArrowRight navigates to next", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsWithPaths} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "ArrowRight" })
        );
      });

      expect(navigateSpy).toHaveBeenCalledWith("/?f=/folder/next", { replace: true });
      component.unmount();
    });

    it("Cmd+[ navigates to prev", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsWithPaths} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "[", metaKey: true })
        );
      });

      expect(navigateSpy).toHaveBeenCalledWith("/?f=/folder/prev", { replace: true });
      component.unmount();
    });

    it("Cmd+] navigates to next", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsWithPaths} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "]", metaKey: true })
        );
      });

      expect(navigateSpy).toHaveBeenCalledWith("/?f=/folder/next", { replace: true });
      component.unmount();
    });

    it("ArrowLeft does not navigate when prevFilePath is null", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsNullPrev} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "ArrowLeft" })
        );
      });

      expect(navigateSpy).not.toHaveBeenCalled();
      component.unmount();
    });

    it("ArrowRight does not navigate when nextFilePath is null", () => {
      const navigateSpy = jest.fn();
      jest.spyOn(useLocation, "default").mockReturnValue({
        location: { search: "" } as Location,
        navigate: navigateSpy
      });

      const component = render(
        <MemoryRouter>
          <ArchivePagination relativeObjects={relativeObjectsNullNext} />
        </MemoryRouter>
      );

      act(() => {
        globalThis.dispatchEvent(
          new KeyboardEvent("keydown", { bubbles: true, cancelable: true, key: "ArrowRight" })
        );
      });

      expect(navigateSpy).not.toHaveBeenCalled();
      component.unmount();
    });
  });
});
