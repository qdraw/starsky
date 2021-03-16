import { shallow } from "enzyme";
import React from "react";
import {
  IFileIndexItem,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "./list-image-child-item";

describe("FlatListItem", () => {
  it("renders", () => {
    shallow(<ListImageChildItem {...newIFileIndexItem()} />);
  });

  it("check if name exist", () => {
    const data = { fileName: "test" } as IFileIndexItem;
    const component = shallow(<ListImageChildItem {...data} />);

    expect(component.exists(".name")).toBeTruthy();
    expect(component.find(".name").text()).toBe("test");
  });

  it("check if tags exist", () => {
    const data = { tags: "test" } as IFileIndexItem;
    const component = shallow(<ListImageChildItem {...data} />);

    expect(component.exists(".tags")).toBeTruthy();
    expect(component.find(".tags").text()).toBe("test");
  });
});
