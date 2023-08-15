import { render } from "@testing-library/react";
import NotFoundPage from "./not-found-page";

describe("NotFoundPage", () => {
  it("has MenuDefault child Component", () => {
    const notFoundComponent = render(<NotFoundPage />);
    const headerText = (
      notFoundComponent.container.querySelector(
        ".content--header"
      ) as HTMLElement
    )?.innerHTML;
    expect(headerText).toContain("Not Found");
  });
});
