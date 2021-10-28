import { render } from "@testing-library/react";
import React from "react";
import * as MenuSearch from "../../components/organisms/menu-search/menu-search";
import MenuMenuSearchContainer from "./menu-search-container";

describe("MenuMenuSearchContainer", () => {
  it("renders", () => {
    render(<MenuMenuSearchContainer />);
  });

  it("expect child object", () => {
    jest.spyOn(MenuSearch, "default").mockImplementationOnce(() => null);
    render(<MenuMenuSearchContainer />);
  });
});
