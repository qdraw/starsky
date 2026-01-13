import { render } from "@testing-library/react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { PageType } from "../../../interfaces/IDetailView";
import { MenuOptionBatchRename } from "./menu-option-batch-rename";

describe("MenuOptionBatchRename", () => {
  const mockState: IArchiveProps = {
    subPath: "/",
    isReadOnly: false,
    collections: true,
    fileIndexItems: [],
    relativeObjects: { nextFilePath: "", prevFilePath: "", nextHash: "", prevHash: "", args: [] },
    breadcrumb: [],
    colorClassActiveList: [],
    colorClassUsage: [],
    collectionsCount: 0,
    pageType: PageType.Archive,
    dateCache: 0
  };

  it("should render menu option when files are selected", () => {
    const { container } = render(
      <MenuOptionBatchRename
        readOnly={false}
        state={mockState}
        select={["/test1.jpg", "/test2.jpg"]}
        dispatch={jest.fn()}
        setSelect={jest.fn()}
      />
    );

    const menuItem = container.querySelector("[data-test='batch-rename']");
    expect(menuItem).toBeInTheDocument();
  });

  it("should be disabled when no files are selected", () => {
    const { container } = render(
      <MenuOptionBatchRename
        readOnly={false}
        state={mockState}
        select={[]}
        dispatch={jest.fn()}
        setSelect={jest.fn()}
      />
    );

    const menuItem = container.querySelector("[data-test='batch-rename']") as HTMLElement;

    expect(menuItem.parentElement?.className).toContain("disabled");
  });

  it("should be disabled in read-only mode", () => {
    const { container } = render(
      <MenuOptionBatchRename
        readOnly={true}
        state={mockState}
        select={["/test1.jpg"]}
        dispatch={jest.fn()}
        setSelect={jest.fn()}
      />
    );

    const menuItem = container.querySelector("[data-test='batch-rename']") as HTMLElement;
    expect(menuItem.parentElement?.className).toContain("disabled");
  });
});
