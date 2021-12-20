import { render } from "@testing-library/react";
import React from "react";
import NotFoundPage from "./not-found-page";

describe("NotFoundPage", () => {
  it("has MenuDefault child Component", () => {
    var notFoundComponent = render(<NotFoundPage></NotFoundPage>);
    var headerText = (
      notFoundComponent.container.querySelector(
        ".content--header"
      ) as HTMLElement
    )?.innerHTML;
    expect(headerText).toContain("Not Found");
  });
});
