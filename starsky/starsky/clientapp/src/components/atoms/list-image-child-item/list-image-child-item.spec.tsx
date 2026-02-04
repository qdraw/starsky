import { render, screen } from "@testing-library/react";
import { IFileIndexItem, newIFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "./list-image-child-item";

describe("FlatListItem", () => {
  it("renders", () => {
    render(<ListImageChildItem {...newIFileIndexItem()} />);
  });

  it("check if name exist", () => {
    const data = { fileName: "test" } as IFileIndexItem;
    const component = render(<ListImageChildItem {...data} />);

    const name = screen.queryAllByTestId("list-image-name")[0];

    expect(name).not.toBeNull();
    expect(name.innerHTML).toBe("test");

    component.unmount();
  });

  it("check if tags exist", () => {
    const data = { tags: "test" } as IFileIndexItem;
    const component = render(<ListImageChildItem {...data} />);

    expect(screen.queryAllByTestId("list-image-tags")).toBeTruthy();
    expect(screen.queryAllByTestId("list-image-tags")[0].innerHTML).toBe("test");

    component.unmount();
  });
});
