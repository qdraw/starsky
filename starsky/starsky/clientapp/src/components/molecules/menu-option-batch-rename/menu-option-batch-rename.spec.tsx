import { render } from "@testing-library/react";
import { MenuOptionBatchRename } from "./menu-option-batch-rename";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { PageType } from "../../../interfaces/IDetailView";

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
        selectedFilePaths={["/test1.jpg", "/test2.jpg"]}
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
        selectedFilePaths={[]}
      />
    );

    const menuItem = container.querySelector("[data-test='batch-rename']") as HTMLElement;
    const button = menuItem?.querySelector("button");
    expect(button?.disabled).toBe(true);
  });

  it("should be disabled in read-only mode", () => {
    const { container } = render(
      <MenuOptionBatchRename
        readOnly={true}
        state={mockState}
        selectedFilePaths={["/test1.jpg"]}
      />
    );

    const menuItem = container.querySelector("[data-test='batch-rename']") as HTMLElement;
    const button = menuItem?.querySelector("button");
    expect(button?.disabled).toBe(true);
  });
});
