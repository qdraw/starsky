import { render, screen } from "@testing-library/react";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ItemTextListView from "./item-text-list-view";

describe("ItemTextListView", () => {
  it("renders (without state component)", () => {
    render(<ItemTextListView fileIndexItems={newIFileIndexItemArray()} callback={() => {}} />);
  });

  it("renders undefined", () => {
    render(<ItemTextListView fileIndexItems={undefined as any} callback={() => {}} />);

    expect(screen.getByTestId("list-text-view-no-photos-in-folder")).toBeTruthy();
  });

  it("list of 1 file item", () => {
    const fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: false
      }
    ] as IFileIndexItem[];
    const list = render(<ItemTextListView fileIndexItems={fileIndexItems} callback={() => {}} />);

    expect(list.container.querySelector("ul li")?.textContent).toBe(fileIndexItems[0].fileName);
  });

  it("list of 1 error item", () => {
    const fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.ServerError,
        isDirectory: false
      }
    ] as IFileIndexItem[];
    const list = render(<ItemTextListView fileIndexItems={fileIndexItems} callback={() => {}} />);

    expect(list.container.querySelector("ul li em")?.textContent).toBe("ServerError");
    expect(list.container.querySelector("ul li")?.textContent).toContain(
      fileIndexItems[0].fileName
    );
  });

  it("list of 1 directory item", () => {
    const fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true
      }
    ] as IFileIndexItem[];

    const callback = jest.fn();
    const list = render(<ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />);

    expect(list.container.querySelector("ul li button")?.textContent).toBe(
      fileIndexItems[0].fileName
    );
  });

  it("list of 1 directory item callback", () => {
    const fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true
      }
    ] as IFileIndexItem[];

    const callback = jest.fn();
    const list = render(<ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />);

    const button = list.container.querySelector("ul li button") as HTMLButtonElement;

    expect(button).not.toBeNull();

    button.click();

    expect(callback).toHaveBeenCalled();
    expect(callback).toHaveBeenCalledWith(fileIndexItems[0].filePath);
  });
});
