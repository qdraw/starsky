import { render } from "@testing-library/react";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import ItemTextListView from "./item-text-list-view";

describe("ItemTextListView", () => {
  it("renders (without state component)", () => {
    render(
      <ItemTextListView
        fileIndexItems={newIFileIndexItemArray()}
        callback={() => {}}
      />
    );
  });

  it("renders undefined", () => {
    var content = render(
      <ItemTextListView fileIndexItems={undefined as any} callback={() => {}} />
    );

    expect(
      content.queryByTestId("list-text-view-no-photos-in-folder")
    ).toBeTruthy();
  });

  it("list of 1 file item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: false
      }
    ] as IFileIndexItem[];
    var list = render(
      <ItemTextListView fileIndexItems={fileIndexItems} callback={() => {}} />
    );

    expect(list.container.querySelector("ul li")?.textContent).toBe(
      fileIndexItems[0].fileName
    );
  });

  it("list of 1 error item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.ServerError,
        isDirectory: false
      }
    ] as IFileIndexItem[];
    var list = render(
      <ItemTextListView fileIndexItems={fileIndexItems} callback={() => {}} />
    );

    expect(list.container.querySelector("ul li em")?.textContent).toBe(
      "ServerError"
    );
    expect(list.container.querySelector("ul li")?.textContent).toContain(
      fileIndexItems[0].fileName
    );
  });

  it("list of 1 directory item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true
      }
    ] as IFileIndexItem[];

    var callback = jest.fn();
    var list = render(
      <ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />
    );

    expect(list.container.querySelector("ul li button")?.textContent).toBe(
      fileIndexItems[0].fileName
    );
  });

  it("list of 1 directory item callback", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true
      }
    ] as IFileIndexItem[];

    var callback = jest.fn();
    var list = render(
      <ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />
    );

    const button = list.container.querySelector(
      "ul li button"
    ) as HTMLButtonElement;

    expect(button).not.toBeNull();

    button.click();

    expect(callback).toBeCalled();
    expect(callback).toBeCalledWith(fileIndexItems[0].filePath);
  });
});
