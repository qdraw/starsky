import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../interfaces/IDetailView";
import * as Link from "../../atoms/link/link";
import IsSearchQueryMenuSearchItem from "./is-search-query-menu-search-item";

describe("IsSearchQueryMenuSearchItem", () => {
  it("renders", () => {
    const history = {
      location: {
        basename: "",
        search: "test"
      },
      navigate: jest.fn()
    } as unknown as IUseLocation;
    render(
      <BrowserRouter>
        <IsSearchQueryMenuSearchItem
          history={history}
          setIsLoading={jest.fn()}
          isSearchQuery={true}
          state={{ fileIndexItem: {} } as IDetailView}
        />
      </BrowserRouter>
    );
  });

  it("should contain test id", () => {
    jest
      .spyOn(Link, "default")
      .mockImplementationOnce((props) => <a data-test={props["data-test"]}></a>);

    const history = {
      location: {
        search: "test"
      }
    } as unknown as IUseLocation;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={true}
        state={{ fileIndexItem: {} } as IDetailView}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();
  });

  it("ctrl click not trigger loading", () => {
    const history = {
      location: {
        basename: "/",
        search: "test"
      }
    } as unknown as IUseLocation;

    const loadingSpy = jest.fn();
    render(
      <BrowserRouter>
        <IsSearchQueryMenuSearchItem
          history={history}
          setIsLoading={loadingSpy}
          isSearchQuery={true}
          state={{ fileIndexItem: {} } as IDetailView}
        />
      </BrowserRouter>
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();

    (id as HTMLElement).dispatchEvent(new MouseEvent("click", { bubbles: true, ctrlKey: true }));

    expect(loadingSpy).not.toHaveBeenCalled();
  });

  it("click trigger loading", () => {
    const history = {
      location: {
        search: "test"
      }
    } as unknown as IUseLocation;
    const loadingSpy = jest.fn();
    render(
      <BrowserRouter>
        <IsSearchQueryMenuSearchItem
          history={history}
          setIsLoading={loadingSpy}
          isSearchQuery={true}
          state={{ fileIndexItem: {} } as IDetailView}
        />
      </BrowserRouter>
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).not.toBeNull();

    (id as HTMLElement).dispatchEvent(new MouseEvent("click", { bubbles: true }));

    expect(loadingSpy).toHaveBeenCalled();
  });

  it("should contain test id delete", () => {
    const history = {
      location: {
        search: "?t=!delete!"
      }
    } as unknown as IUseLocation;
    render(
      <BrowserRouter>
        <IsSearchQueryMenuSearchItem
          history={history}
          setIsLoading={jest.fn()}
          isSearchQuery={true}
          state={{ fileIndexItem: {} } as IDetailView}
        />
      </BrowserRouter>
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
    } as unknown as IUseLocation;
    render(
      <IsSearchQueryMenuSearchItem
        history={history}
        setIsLoading={jest.fn()}
        isSearchQuery={false}
        state={{ fileIndexItem: {} } as IDetailView}
      />
    );

    const id = screen.queryByTestId("menu-detail-view-close");
    expect(id).toBeNull();
  });
});
