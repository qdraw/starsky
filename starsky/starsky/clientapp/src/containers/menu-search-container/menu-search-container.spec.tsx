import { render } from "@testing-library/react";
import * as MenuSearch from "../../components/organisms/menu-search/menu-search";
import MenuMenuSearchContainer from "./menu-search-container";

describe("MenuMenuSearchContainer", () => {
  it("renders", () => {
    const item = render(<MenuMenuSearchContainer />);
    expect(item).toBeTruthy();
  });

  it("expect child object", () => {
    jest.spyOn(MenuSearch, "default").mockImplementationOnce(() => null);
    const item = render(<MenuMenuSearchContainer />);
    expect(item).toBeTruthy();
  });
});
