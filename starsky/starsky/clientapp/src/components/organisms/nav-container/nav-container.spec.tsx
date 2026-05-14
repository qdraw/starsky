import { render } from "@testing-library/react";
import NavContainer from "./nav-container";

describe("NavContainer", () => {
  it("renders", () => {
    const item = render(<NavContainer hamburgerMenu={true}>content</NavContainer>);
    expect(item).toBeTruthy();
  });
});
