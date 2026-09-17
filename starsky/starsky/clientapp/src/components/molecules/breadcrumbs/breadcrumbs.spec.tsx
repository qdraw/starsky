import { fireEvent, screen } from "@testing-library/dom";
import { render } from "@testing-library/react";
import * as MoveFileHelper from "../../../shared/move-file-helper";
import * as Link from "../../atoms/link/link";
import Breadcrumb from "./breadcrumbs";

jest.mock("../../../shared/move-file-helper", () => ({
  ...jest.requireActual("../../../shared/move-file-helper"),
  moveDragAndDropFiles: jest.fn().mockResolvedValue(true)
}));

describe("Breadcrumb", () => {
  it("renders", () => {
    jest.spyOn(Link, "default").mockImplementationOnce(() => <a></a>);
    const item = render(<Breadcrumb subPath="/" breadcrumb={["/"]} />);
    expect(item).toBeTruthy();
  });

  it("disabled", () => {
    const wrapper = render(<Breadcrumb subPath="" breadcrumb={[]} />);

    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(0);

    wrapper.unmount();
  });

  it("check Length for breadcrumbs", () => {
    jest
      .spyOn(Link, "default")
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>);

    const breadcrumbs = ["/", "/test"];
    const wrapper = render(<Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />);
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(2);

    wrapper.unmount();
  });

  it("check 3 Length for breadcrumbs", () => {
    jest
      .spyOn(Link, "default")
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>);

    const breadcrumbs = ["/", "/test", "/01"];
    const wrapper = render(<Breadcrumb subPath="/test/01/01" breadcrumb={breadcrumbs} />);
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(3);

    wrapper.unmount();
  });

  describe("drag-and-drop", () => {
    const dragData = JSON.stringify({ filePaths: ["/photos/a.jpg"], folderPaths: [] });
    const MIME = MoveFileHelper.DRAG_MOVE_MIME;

    function makeDragEvent(type: string, data?: string) {
      return Object.assign(new Event(type, { bubbles: true, cancelable: true }), {
        dataTransfer: {
          types: data ? [MIME] : [],
          getData: (_mime: string) => data ?? "",
          setData: jest.fn(),
          dropEffect: "none",
          effectAllowed: "none"
        } as unknown as DataTransfer
      });
    }

    it("dragOver with starsky-move data adds drag-over class", () => {
      jest.spyOn(Link, "default").mockImplementation(() => <a></a>);
      const { container } = render(<Breadcrumb subPath="/photos/sub" breadcrumb={["/", "/photos"]} />);
      const span = screen.queryAllByTestId("breadcrumb-span")[0];

      fireEvent(span, makeDragEvent("dragover", dragData));

      const dragOverSpan = container.querySelector(".breadcrumb__item--drag-over");
      expect(dragOverSpan).not.toBeNull();
    });

    it("dragLeave removes drag-over class", () => {
      jest.spyOn(Link, "default").mockImplementation(() => <a></a>);
      const { container } = render(<Breadcrumb subPath="/photos/sub" breadcrumb={["/", "/photos"]} />);
      const span = screen.queryAllByTestId("breadcrumb-span")[0];

      fireEvent(span, makeDragEvent("dragover", dragData));
      fireEvent(span, makeDragEvent("dragleave"));

      const dragOverSpan = container.querySelector(".breadcrumb__item--drag-over");
      expect(dragOverSpan).toBeNull();
    });

    it("drop calls moveDragAndDropFiles with correct target path", async () => {
      const mockMove = jest.spyOn(MoveFileHelper, "moveDragAndDropFiles").mockResolvedValue(true);
      jest.spyOn(Link, "default").mockImplementation(() => <a></a>);
      render(<Breadcrumb subPath="/photos/sub" breadcrumb={["/", "/photos"]} />);
      const spans = screen.queryAllByTestId("breadcrumb-span");

      fireEvent(spans[0], makeDragEvent("drop", dragData));

      await new Promise((r) => setTimeout(r, 0));
      expect(mockMove).toHaveBeenCalledWith(["/photos/a.jpg"], [], "/");
    });

    it("dragOver without starsky-move data does not add class", () => {
      jest.spyOn(Link, "default").mockImplementation(() => <a></a>);
      const { container } = render(<Breadcrumb subPath="/photos/sub" breadcrumb={["/", "/photos"]} />);
      const span = screen.queryAllByTestId("breadcrumb-span")[0];

      fireEvent(span, makeDragEvent("dragover"));

      expect(container.querySelector(".breadcrumb__item--drag-over")).toBeNull();
    });
  });
});
