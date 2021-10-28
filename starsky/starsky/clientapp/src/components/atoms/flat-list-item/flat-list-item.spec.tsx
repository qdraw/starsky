import { render } from "@testing-library/react";
import {
  IFileIndexItem,
  ImageFormat,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import FlatListItem from "./flat-list-item";

describe("FlatListItem", () => {
  it("renders", () => {
    render(
      <FlatListItem
        item={newIFileIndexItem()}
        onSelectionCallback={jest.fn()}
      />
    );
  });

  it("check if name exist", () => {
    const data = { fileName: "test" } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName("name");

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("test");
  });

  it("check if lastedited exist", () => {
    const data = { lastEdited: "2021-01-30T16:26:43.776883" } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName(
      "lastedited"
    );

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("30-1-2021 16:26:43");
  });

  it("check if size exist and returns dash dash", () => {
    const data = { size: 0 } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName("size");

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("--");
  });

  it("check if size returns value", () => {
    const data = { size: 10 } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName("size");

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("10 Bytes");
  });

  it("check if imageformat exist and returns dash dash", () => {
    const data = { imageFormat: ImageFormat.unknown } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName(
      "imageformat"
    );

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("--");
  });

  it("check if imageFormat returns value", () => {
    const data = { imageFormat: ImageFormat.jpg } as IFileIndexItem;
    const component = render(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    const nameComponents = component.container.getElementsByClassName(
      "imageformat"
    );

    expect(nameComponents.length).toBe(1);
    expect(nameComponents[0].innerHTML).toBe("jpg");
  });
});
