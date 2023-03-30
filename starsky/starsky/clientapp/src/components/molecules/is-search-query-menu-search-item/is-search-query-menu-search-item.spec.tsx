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

  it("ctrl click not trigger loading", () => {
    const history = {
      location: {
        search: "test"
      }
    } as any;
    const loadingSpy = jest.fn();
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={loadingSpy}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as any}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();

    (id as HTMLElement).dispatchEvent(
      new MouseEvent("click", { bubbles: true, ctrlKey: true })
    );

    expect(loadingSpy).not.toBeCalled();
  });

  it("click trigger loading", () => {
    const history = {
      location: {
        search: "test"
      }
    } as any;
    const loadingSpy = jest.fn();
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={loadingSpy}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as any}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();

    (id as HTMLElement).dispatchEvent(
      new MouseEvent("click", { bubbles: true })
    );

    expect(loadingSpy).toBeCalled();
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
