import { fireEvent, render } from "@testing-library/react";
import { GoToParentFolder } from "./go-to-parent-folder";

describe("GoToParentFolder", () => {
  it("should render when isSearchQuery is true and click", () => {
    // Mock the history object
    const history = {
      navigate: jest.fn(),
      location: {
        search: "someSearchQuery"
      }
    };

    const state = {
      fileIndexItem: {
        parentDirectory: "parentDir",
        filePath: "filePath"
      }
    } as any; // Define your state object as per your interface

    const { getByTestId } = render(
      <GoToParentFolder
        isSearchQuery={true}
        history={history as any}
        state={state as any}
      />
    );

    const parentFolderLink = getByTestId("go-to-parent-folder");

    fireEvent.click(parentFolderLink);

    expect(history.navigate).toHaveBeenCalledWith(expect.any(String), {
      state: { filePath: "filePath" }
    });
  });

  it("should render when isSearchQuery is true and keyDown", () => {
    // Mock the history object
    const history = {
      navigate: jest.fn(),
      location: {
        search: "someSearchQuery"
      }
    };

    const state = {
      fileIndexItem: {
        parentDirectory: "parentDir",
        filePath: "filePath"
      }
    } as any; // Define your state object as per your interface

    const { getByTestId } = render(
      <GoToParentFolder
        isSearchQuery={true}
        history={history as any}
        state={state as any}
      />
    );

    const parentFolderLink = getByTestId("go-to-parent-folder");

    fireEvent.keyDown(parentFolderLink, { key: "Enter" });

    expect(history.navigate).toHaveBeenCalledWith(expect.any(String), {
      state: { filePath: "filePath" }
    });
  });

  it("should not render when isSearchQuery is false", () => {
    const { container } = render(
      <GoToParentFolder
        isSearchQuery={false}
        history={null as any}
        state={null as any}
      />
    );

    expect(container.firstChild).toBeNull();
  });
});
