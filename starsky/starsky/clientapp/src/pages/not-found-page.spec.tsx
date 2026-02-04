import { render } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { NotFoundPage } from "./not-found-page";

describe("NotFoundPage", () => {
  it("has MenuDefault child Component", () => {
    jest.spyOn(console, "error").mockImplementationOnce(() => {});

    const notFoundComponent = render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    );
    const headerText = (
      notFoundComponent.container.querySelector(".content--header") as HTMLElement
    )?.innerHTML;
    expect(headerText).toContain("Not Found");
  });
});
