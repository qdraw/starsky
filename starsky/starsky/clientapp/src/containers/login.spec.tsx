import { globalHistory } from "@reach/router";
import { fireEvent, render, waitFor } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useFetch from "../hooks/use-fetch";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import * as FetchPost from "../shared/fetch-post";
import { UrlQuery } from "../shared/url-query";
import Login from "./login";

describe("Login", () => {
  it("renders", () => {
    render(<Login />);
  });

  it("account already logged in", () => {
    globalHistory.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    var login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    const err = login.queryByTestId("logout-content");
    expect(err).toBeTruthy();

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account already logged in special return url", () => {
    globalHistory.navigate("/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    var login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(login.queryByTestId("logout")).toBeTruthy();
    expect(login.queryAllByTestId("stayLoggedin")).toBeTruthy();

    // no prefix (starsky in url)
    expect((login.queryByTestId("logout") as HTMLAnchorElement).href).toBe(
      "http://localhost/account/logout?ReturnUrl=/test"
    );

    expect(
      (login.queryAllByTestId("stayLoggedin")[0] as HTMLAnchorElement).href
    ).toBe("http://localhost/test");

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account logged in /starsky - return url", () => {
    globalHistory.navigate("/starsky/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    var login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(login.queryByTestId("logout")).toBeTruthy();
    expect(login.queryAllByTestId("stayLoggedin")).toBeTruthy();

    // including starsky prefix
    expect((login.queryByTestId("logout") as HTMLAnchorElement).href).toBe(
      "http://localhost/starsky/account/logout?ReturnUrl=/starsky/test"
    );

    expect(
      (login.queryAllByTestId("stayLoggedin")[0] as HTMLAnchorElement).href
    ).toBe("http://localhost/starsky/test");

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account not logged in", () => {
    globalHistory.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    var login = render(<Login />);

    expect(login.queryByTestId("email")).toBeTruthy();
    expect(login.queryByTestId("password")).toBeTruthy();

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account 406 UrlAccountRegister", () => {
    jest.spyOn(useFetch, "default").mockReset();
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 406 } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    var login = render(<Login />);

    expect(
      globalHistory.location.pathname.indexOf(
        new UrlQuery().UrlAccountRegisterApi()
      )
    ).toBeTruthy();
    expect(useFetchSpy).toBeCalled();

    act(() => {
      login.unmount();
      globalHistory.navigate("/");
    });
  });

  it("login flow succesfull", () => {
    globalHistory.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const mockPost: Promise<any> = Promise.resolve({
      statusCode: 200,
      data: "ok"
    });
    var postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost);

    var login = render(<Login />);

    // email
    act(() => {
      const emailElement = login.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = login.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = login.queryByTestId("login-content");
    act(() => {
      loginContent?.querySelector("form")?.submit();
    });

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    expect(postSpy).toBeCalled();
    expect(postSpy).toBeCalledWith(
      new UrlQuery().UrlLoginApi(),
      "Email=dont@mail.me&Password=password"
    );

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("login flow fail by backend (401)", async () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const mockPost: Promise<any> = Promise.resolve({
      statusCode: 401,
      data: "fail"
    });
    var postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost)
      .mockImplementationOnce(() => mockPost);

    var login = render(<Login />);

    // email
    act(() => {
      const emailElement = login.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = login.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = login.queryByTestId("login-content");
    act(() => {
      loginContent?.querySelector("form")?.submit();
    });

    await login.findByTestId("login-error");
    expect(useFetchSpy).toBeCalled();
    expect(postSpy).toBeCalled();

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("login flow fail by backend (423)", async () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const mockPost: Promise<any> = Promise.resolve({
      statusCode: 423,
      data: "fail"
    });
    var postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost);

    var login = render(<Login />);

    // email
    act(() => {
      const emailElement = login.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = login.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = login.queryByTestId("login-content");
    act(() => {
      loginContent?.querySelector("form")?.submit();
    });

    // expect(login.html().search('class="content--error-true"')).toBeTruthy();
    // expect(login.queryByTestId("login-error")).toBeTruthy();
    await login.findByTestId("login-error");
    expect(useFetchSpy).toBeCalled();
    expect(postSpy).toBeCalled();

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });
});
