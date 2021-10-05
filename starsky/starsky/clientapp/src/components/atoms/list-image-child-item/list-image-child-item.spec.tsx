import { render } from "@testing-library/react";
import React from "react";
import {
  IFileIndexItem,
  newIFileIndexItem
} from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "./list-image-child-item";

describe("FlatListItem", () => {
  it("renders", () => {
    render(<ListImageChildItem {...newIFileIndexItem()} />);
  });

  it("check if name exist", () => {
    const data = { fileName: "test" } as IFileIndexItem;
    const component = render(<ListImageChildItem {...data} />);

    expect(component.exists(".name")).toBeTruthy();
    expect(component.find(".name").text()).toBe("test");
  });

  it("check if tags exist", () => {
    const data = { tags: "test" } as IFileIndexItem;
    const component = render(<ListImageChildItem {...data} />);

    expect(component.queryAllByTestId("list-image-tags")).toBeTruthy();
    expect(component.queryAllByTestId("list-image-tags")[0].innerText).toBe(
      "test"
    );
  });
});
