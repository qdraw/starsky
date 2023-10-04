import { render } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import { MemoryRouter } from "react-router-dom";
import { Router } from "../../../router-app/router-app";
import SearchPagination from "./search-pagination";
describe("SearchPagination", () => {
  it("renders", () => {
    render(<SearchPagination />);
  });

  it("next page exist", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      Router.navigate("/?p=0");
    });

    const component = render(
      <MemoryRouter>
        <SearchPagination lastPageNumber={2}></SearchPagination>
      </MemoryRouter>
    );

    const nextButton = component.queryByTestId(
      "search-pagination-next"
    ) as HTMLAnchorElement;
    expect(nextButton).toBeTruthy();
    expect(nextButton.href).toBe("http://localhost/?p=1");

    const prevButton = component.queryByTestId(
      "search-pagination-prev"
    ) as HTMLAnchorElement;
    expect(prevButton).toBeFalsy();
  });

  it("prev page exist", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      Router.navigate("/?p=1");
    });

    const component = render(
      <MemoryRouter>
        <SearchPagination lastPageNumber={2}></SearchPagination>
      </MemoryRouter>
    );

    const prevButton = component.queryByTestId(
      "search-pagination-prev"
    ) as HTMLAnchorElement;
    expect(prevButton).toBeTruthy();

    expect(prevButton.href).toBe("http://localhost/?p=0");
  });

  it("prev page exist + remove select param", () => {
    // due the fact that the selected item does not exist on that new page

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      Router.navigate("/?p=1&select=test");
    });

    const component = render(
      <MemoryRouter>
        <SearchPagination lastPageNumber={2}></SearchPagination>
      </MemoryRouter>
    );

    const prevButton = component.queryByTestId(
      "search-pagination-prev"
    ) as HTMLAnchorElement;
    expect(prevButton).toBeTruthy();

    expect(prevButton.href).toBe("http://localhost/?p=0&select=");
  });

  it("next page exist + remove select param", () => {
    // due the fact that the selected item does not exist on that new page
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      Router.navigate("/?p=0&select=test");
    });

    const component = render(
      <MemoryRouter>
        <SearchPagination lastPageNumber={2}></SearchPagination>
      </MemoryRouter>
    );

    const nextButton = component.queryByTestId(
      "search-pagination-next"
    ) as HTMLAnchorElement;
    expect(nextButton).toBeTruthy();
    expect(nextButton.href).toBe("http://localhost/?p=1&select=");
  });
});
