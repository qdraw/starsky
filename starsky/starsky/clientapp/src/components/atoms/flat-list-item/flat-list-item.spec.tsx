import { shallow } from "enzyme";
import {
  IFileIndexItem,
  ImageFormat,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import FlatListItem from "./flat-list-item";

describe("FlatListItem", () => {
  it("renders", () => {
    shallow(
      <FlatListItem
        item={newIFileIndexItem()}
        onSelectionCallback={jest.fn()}
      />
    );
  });

  it("check if name exist", () => {
    const data = { fileName: "test" } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".name")).toBeTruthy();
    expect(component.find(".name").text()).toBe("test");
  });

  it("check if lastedited exist", () => {
    const data = { lastEdited: "2021-01-30T16:26:43.776883" } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".lastedited")).toBeTruthy();
    expect(component.find(".lastedited").text()).toBe("30-1-2021 16:26:43");
  });

  it("check if size exist and returns dash dash", () => {
    const data = { size: 0 } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".size")).toBeTruthy();
    expect(component.find(".size").text()).toBe("--");
  });

  it("check if size returns value", () => {
    const data = { size: 10 } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".size")).toBeTruthy();
    expect(component.find(".size").text()).toBe("10 Bytes");
  });

  it("check if imageformat exist and returns dash dash", () => {
    const data = { imageFormat: ImageFormat.unknown } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".imageformat")).toBeTruthy();
    expect(component.find(".imageformat").text()).toBe("--");
  });

  it("check if imageFormat returns value", () => {
    const data = { imageFormat: ImageFormat.jpg } as IFileIndexItem;
    const component = shallow(
      <FlatListItem item={data} onSelectionCallback={jest.fn()} />
    );

    expect(component.exists(".imageformat")).toBeTruthy();
    expect(component.find(".imageformat").text()).toBe("jpg");
  });
});
