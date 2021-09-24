import { globalHistory } from "@reach/router";
import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import SearchPagination from "./search-pagination";

describe("SearchPagination", () => {
  it("renders", () => {
    shallow(<SearchPagination />);
  });

  it("next page exist", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?p=0");
    });

    var component = mount(
      <SearchPagination lastPageNumber={2}>t</SearchPagination>
    );
    expect(component.find("a.next").props().href).toBe("/?p=1");
  });

  it("prev page exist", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?p=1");
    });

    var component = render(
      <SearchPagination lastPageNumber={2}>t</SearchPagination>
    );
    expect(component.find("a.prev").props().href).toBe("/?p=0");
  });

  it("prev page exist + remove select param", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?p=1&select=test");
    });

    var component = render(
      <SearchPagination lastPageNumber={2}>t</SearchPagination>
    );

    expect(component.find("a.prev").props().href).toBe("/?p=0&select=");
  });

  it("next page exist + remove select param", () => {
    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?p=0&select=test");
    });

    var component = render(
      <SearchPagination lastPageNumber={2}>t</SearchPagination>
    );
    expect(component.find("a.next").props().href).toBe("/?p=1&select=");
  });
});
