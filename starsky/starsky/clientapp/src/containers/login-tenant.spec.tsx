import { fireEvent, screen } from "@testing-library/dom";
import { act, render } from "@testing-library/react";
import * as useFetch from "../hooks/use-fetch";
import * as UseTenants from "../hooks/use-tenants/use-tenants";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import { newIArchive } from "../interfaces/IArchive";
import { Router } from "../router-app/router-app";
import * as FetchPost from "../shared/fetch/fetch-post";
import { FileListCache } from "../shared/filelist-cache";
import { Login } from "./login";

describe("Login tenant landing", () => {
  const login = async () => {
    jest
      .spyOn(useFetch, "default")
      .mockImplementation(() => ({ statusCode: 401, data: "false" }) as IConnectionDefault);
    jest.spyOn(FetchPost, "default").mockResolvedValue({ statusCode: 200, data: "ok" });

    render(<Login />);
    fireEvent.change(screen.getByTestId("email"), { target: { value: "a@b.nl" } });
    fireEvent.change(screen.getByTestId("password"), { target: { value: "password-1234" } });
    await act(async () => {
      fireEvent.click(screen.getByTestId("login-submit"));
    });
  };

  afterEach(() => {
    jest.restoreAllMocks();
    sessionStorage.clear();
  });

  it("clears the cached folders of the previous user after a login", async () => {
    Router.navigate("/");
    new FileListCache().CacheSet("?f=/", newIArchive());
    expect(sessionStorage).toHaveLength(1);
    jest.spyOn(UseTenants, "FetchLandingTenant").mockResolvedValue(null);

    await login();

    expect(sessionStorage).toHaveLength(0);
  });

  it("clears the cached folders when nobody is logged in (login form)", () => {
    Router.navigate("/");
    new FileListCache().CacheSet("?f=/", newIArchive());
    jest
      .spyOn(useFetch, "default")
      .mockImplementation(() => ({ statusCode: 401, data: "false" }) as IConnectionDefault);

    render(<Login />);

    expect(sessionStorage).toHaveLength(0);
  });

  it("goes to the tenant used last, in a full page load", async () => {
    Router.navigate("/");
    const landing = jest.spyOn(UseTenants, "FetchLandingTenant").mockResolvedValue("acme");
    const goTo = jest.spyOn(UseTenants, "GoToTenant").mockImplementation(() => {});

    await login();

    expect(landing).toHaveBeenCalled();
    expect(goTo).toHaveBeenCalledWith("acme");
  });

  it("stays when there is no other landing tenant", async () => {
    Router.navigate("/");
    jest.spyOn(UseTenants, "FetchLandingTenant").mockResolvedValue(null);
    const goTo = jest.spyOn(UseTenants, "GoToTenant").mockImplementation(() => {});

    await login();

    expect(goTo).not.toHaveBeenCalled();
  });

  it("the ReturnUrl of the logout link is not a real target", async () => {
    Router.navigate("/?ReturnUrl=%2F%3Ff%3D%2F");
    jest.spyOn(UseTenants, "FetchLandingTenant").mockResolvedValue("acme");
    const goTo = jest.spyOn(UseTenants, "GoToTenant").mockImplementation(() => {});

    await login();

    expect(goTo).toHaveBeenCalledWith("acme");
  });

  it("an explicit ReturnUrl wins over the landing tenant", async () => {
    Router.navigate("/?ReturnUrl=/starsky/trash");
    const landing = jest.spyOn(UseTenants, "FetchLandingTenant").mockResolvedValue("acme");
    const goTo = jest.spyOn(UseTenants, "GoToTenant").mockImplementation(() => {});

    await login();

    expect(landing).not.toHaveBeenCalled();
    expect(goTo).not.toHaveBeenCalled();
  });
});
