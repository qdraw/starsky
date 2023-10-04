import { render } from "@testing-library/react";
import * as Import from "../containers/import/import";
import * as Login from "../containers/login";
import * as MediaContent from "../containers/media-content";
import * as Preferences from "../containers/preferences/preferences";
import * as useSearchList from "../hooks/use-searchlist";
import { ISearchList } from "../hooks/use-searchlist";
import * as NotFoundPage from "../pages/not-found-page";
import * as SearchPage from "../pages/search-page";
import * as TrashPage from "../pages/trash-page";
import RouterApp, { Router, RoutesConfig } from "./router-app";

describe("Router", () => {
  it("default", () => {
    const searchPagePageSpy = jest
      .spyOn(MediaContent, "default")
      .mockImplementationOnce(() => <></>);

    const component = render(<RouterApp />);

    expect(
      RoutesConfig.find((x) => x.path === "/")?.element
    ).not.toBeUndefined();

    expect(searchPagePageSpy).toBeCalled();

    component.unmount();
  });

  it("search", () => {
    jest.spyOn(SearchPage, "SearchPage").mockImplementationOnce(() => <></>);

    const searchListMock = jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => {
        return [{} as ISearchList, () => {}] as any;
      });

    Router.navigate("search?q=t");

    const component = render(<RouterApp />);

    expect(searchListMock).toBeCalled();
    expect(searchListMock).toBeCalledWith(undefined, undefined, true);

    component.unmount();
  });

  it("TrashPage", () => {
    const searchListMock = jest
      .spyOn(useSearchList, "default")
      .mockReset()
      .mockImplementationOnce(() => {
        return [{} as ISearchList, () => {}] as any;
      });

    jest.spyOn(TrashPage, "TrashPage").mockImplementationOnce(() => {
      return <></>;
    });

    Router.navigate("/trash?q=t");

    const component = render(<RouterApp></RouterApp>);

    expect(searchListMock).toBeCalled();
    expect(searchListMock).toBeCalledWith("!delete!", undefined, true);

    component.unmount();
  });

  it("ImportPage", () => {
    const importPagePageSpy = jest
      .spyOn(Import, "Import")
      .mockImplementationOnce(() => {
        return <></>;
      });

    Router.navigate("/import");

    const component = render(<RouterApp></RouterApp>);

    expect(importPagePageSpy).toBeCalled();

    component.unmount();
  });

  it("LoginPage", () => {
    const loginPagePageSpy = jest
      .spyOn(Login, "Login")
      .mockImplementationOnce(() => <></>);

    Router.navigate("/account/login");

    const component = render(<RouterApp></RouterApp>);

    console.log(component.container.innerHTML);

    expect(loginPagePageSpy).toBeCalled();

    component.unmount();
  });

  it("PreferencesPage", () => {
    const preferencesPagePageSpy = jest
      .spyOn(Preferences, "Preferences")
      .mockImplementationOnce(() => {
        return <></>;
      });

    Router.navigate("/preferences");

    const component = render(<RouterApp></RouterApp>);

    expect(preferencesPagePageSpy).toBeCalled();

    component.unmount();
  });

  it("NotFoundPage", () => {
    jest
      .spyOn(NotFoundPage, "NotFoundPage")
      .mockImplementationOnce(() => <></>);

    Router.navigate("/not-found");
    const component = render(<RouterApp></RouterApp>);

    expect(component.queryByTestId("not-found-page")).not.toBeNull();

    component.unmount();
  });
});
