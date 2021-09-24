import { render } from "@testing-library/react";
import React from "react";
import HamburgerMenuToggle from "./hamburger-menu-toggle";

describe("HamburgerMenuToggle", () => {
  it("renders", () => {
    render(
      <HamburgerMenuToggle
        select={false}
        hamburgerMenu={true}
        setHamburgerMenu={jest.fn()}
      />
    );
  });
});
