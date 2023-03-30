import { render, screen } from "@testing-library/react";
import IsSearchQueryMenuSearchItem from "./is-search-query-menu-search-item";

describe("IsSearchQueryMenuSearchItem", () => {
  it("renders", () => {
    const history = {
      location: {
        search: "test"
      }
    } as any;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as any}
      />
    );
  });

  it("should contain test id", () => {
    const history = {
      location: {
        search: "test"
      }
    } as any;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as any}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();
  });

  it("should contain test id delete", () => {
    const history = {
      location: {
        search: "?t=!delete!"
      }
    } as any;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as any}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");

    expect(id?.innerHTML).toBe("Trash");
    expect(id).not.toBeNull();
  });

  it("should not contain test id", () => {
    const history = {
      location: {
        search: "test"
      }
    } as any;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={false}
        state={{ fileIndexItem: {} } as any}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).toBeNull();
  });
});
