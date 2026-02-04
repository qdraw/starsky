import { fireEvent, render } from "@testing-library/react";
import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../../interfaces/IDetailView";
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
    } as IDetailView; // Define your state object as per your interface

    const { getByTestId } = render(
      <GoToParentFolder
        isSearchQuery={true}
        history={history as unknown as IUseLocation}
        state={state as IDetailView}
      />
    );

    const parentFolderLink = getByTestId("go-to-parent-folder");

    fireEvent.click(parentFolderLink);

    expect(history.navigate).toHaveBeenCalledWith(expect.any(String), {
      state: { filePath: "filePath" }
    });
  });

  it("should render when isSearchQuery is true and keyDown enter", () => {
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
    } as IDetailView; // Define your state object as per your interface

    const { getByTestId } = render(
      <GoToParentFolder
        isSearchQuery={true}
        history={history as unknown as IUseLocation}
        state={state as IDetailView}
      />
    );

    const parentFolderLink = getByTestId("go-to-parent-folder");

    fireEvent.keyDown(parentFolderLink, { key: "Enter" });

    expect(history.navigate).toHaveBeenCalledWith(expect.any(String), {
      state: { filePath: "filePath" }
    });
  });

  it("should render when isSearchQuery is true and keyDown tab so skip", () => {
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
    } as unknown; // Define your state object as per your interface

    const { getByTestId } = render(
      <GoToParentFolder
        isSearchQuery={true}
        history={history as unknown as IUseLocation}
        state={state as IDetailView}
      />
    );

    const parentFolderLink = getByTestId("go-to-parent-folder");

    fireEvent.keyDown(parentFolderLink, { key: "Tab" });

    expect(history.navigate).toHaveBeenCalledTimes(0);
  });

  it("should not render when isSearchQuery is false", () => {
    const { container } = render(
      <GoToParentFolder
        isSearchQuery={false}
        history={null as unknown as IUseLocation}
        state={null as unknown as IDetailView}
      />
    );

    expect(container.firstChild).toBeNull();
  });
});
