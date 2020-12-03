import { shallow } from "enzyme";
import React from "react";
import * as MenuSearch from "../../components/organisms/menu-search/menu-search";
import MenuMenuSearchContainer from "./menu-search-container";

describe("MenuMenuSearchContainer", () => {
  it("renders", () => {
    shallow(<MenuMenuSearchContainer />);
  });

  it("expect child object", () => {
    jest.spyOn(MenuSearch, "default").mockImplementationOnce(() => null);
    shallow(<MenuMenuSearchContainer />);
  });
});
