import { render } from "@testing-library/react";
import { RouterProvider, createMemoryRouter } from "react-router-dom";
import * as ContentPage from "../pages/content-page";
import * as ImportPage from "../pages/import-page";
import * as LoginPage from "../pages/login-page";
import * as NotFoundPage from "../pages/not-found-page";
import * as SearchPage from "../pages/search-page";
import * as TrashPage from "../pages/trash-page";
import RouterApp, { RoutesConfig } from "./router-app";
describe("Router", () => {
  it("default", () => {
    const contentPageSpy = jest
      .spyOn(ContentPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });

    const memoryRouter = createMemoryRouter(RoutesConfig, {
      initialEntries: ["/"]
    });

    const component = render(<RouterProvider router={memoryRouter} />);

    console.log(component.container.innerHTML);

    expect(contentPageSpy).toBeCalled();
  });

  xit("search", () => {
    const searchPagePageSpy = jest
      .spyOn(SearchPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    window.location.replace("/search?q=t");
    render(<RouterApp></RouterApp>);
    expect(searchPagePageSpy).toBeCalled();
  });

  xit("TrashPage", () => {
    const trashPagePageSpy = jest
      .spyOn(TrashPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    window.location.replace("/trash?q=t");
    render(<RouterApp></RouterApp>);
    expect(trashPagePageSpy).toBeCalled();
  });

  xit("ImportPage", () => {
    const importPagePageSpy = jest
      .spyOn(ImportPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    window.location.replace("/import?q=t");
    render(<RouterApp></RouterApp>);
    expect(importPagePageSpy).toBeCalled();
  });

  xit("LoginPage", () => {
    const loginPagePageSpy = jest
      .spyOn(LoginPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    window.location.replace("/account/login");
    render(<RouterApp></RouterApp>);
    expect(loginPagePageSpy).toBeCalled();
  });

  xit("NotFoundPage", () => {
    const notFoundPageSpy = jest
      .spyOn(NotFoundPage, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    window.location.replace("/not-found");
    render(<RouterApp></RouterApp>);
    expect(notFoundPageSpy).toBeCalled();
  });
});
