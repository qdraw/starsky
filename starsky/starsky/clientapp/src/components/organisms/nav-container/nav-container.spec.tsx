import React from "react";
import NavContainer from "./nav-container";

describe("NavContainer", () => {
  it("renders", () => {
    render(<NavContainer hamburgerMenu={true}>content</NavContainer>);
  });
});
