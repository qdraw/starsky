import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import * as useFetch from "../hooks/use-fetch";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import { Router } from "../router-app/router-app";
import * as FetchPost from "../shared/fetch-post";
import { UrlQuery } from "../shared/url-query";
import { Login } from "./login";

describe("Login", () => {
  it("renders", () => {
    render(<Login />);
  });

  it("account already logged in", () => {
    Router.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    const err = screen.queryByTestId("logout-content");
    expect(err).toBeTruthy();

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });

  it("database error message-database-connection", () => {
    Router.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 500,
      data: ""
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const view = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    const err = screen.queryByTestId("message-database-connection");
    expect(err).toBeTruthy();

    act(() => {
      Router.navigate("/");
      view.unmount();
    });
  });

  it("account already logged in special return url", () => {
    Router.navigate("/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("logout")).toBeTruthy();
    expect(screen.queryAllByTestId("stayLoggedin")).toBeTruthy();

    // no prefix (starsky in url)
    expect((screen.queryByTestId("logout") as HTMLAnchorElement).href).toBe(
      "http://localhost/account/logout?ReturnUrl=/test"
    );

    expect(
      (login.queryAllByTestId("stayLoggedin")[0] as HTMLAnchorElement).href
    ).toBe("http://localhost/test");

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });

  it("account logged in /starsky - return url", () => {
    Router.navigate("/starsky/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = {
      statusCode: 200,
      data: "true"
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const login = render(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(screen.getByTestId("logout")).toBeTruthy();
    expect(login.queryAllByTestId("stayLoggedin")).toBeTruthy();

    // including starsky prefix
    expect((screen.getByTestId("logout") as HTMLAnchorElement).href).toBe(
      "http://localhost/starsky/account/logout?ReturnUrl=/starsky/test"
    );

    expect(
      (login.queryAllByTestId("stayLoggedin")[0] as HTMLAnchorElement).href
    ).toBe("http://localhost/starsky/test");

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });

  it("account not logged in", () => {
    Router.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const login = render(<Login />);

    expect(screen.getByTestId("email")).toBeTruthy();
    expect(screen.getByTestId("password")).toBeTruthy();

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(
      new UrlQuery().UrlAccountStatus(),
      "get"
    );

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });

  it("account 406 UrlAccountRegister", () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 406 } as IConnectionDefault;
    const connectionDefaultExample2 = { statusCode: 200 } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample2)
      .mockImplementationOnce(() => connectionDefaultExample2);

    const login = render(<Login />);

    expect(
      window.location.pathname.indexOf(new UrlQuery().UrlAccountRegisterApi())
    ).toBeTruthy();
    expect(useFetchSpy).toBeCalled();

    act(() => {
      login.unmount();
      Router.navigate("/");
    });
  });

  it("login flow succesfull", () => {
    Router.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample);

    const mockPost: Promise<any> = Promise.resolve({
      statusCode: 200,
      data: "ok"
    });
    const postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost);

    const login = render(<Login />);

    // email
    act(() => {
      const emailElement = screen.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = screen.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = screen.queryByTestId("login-content");
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
      Router.navigate("/");
      login.unmount();
    });
  });

  it("login flow fail by backend (401)", async () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    const useFetchSpy = jest
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
    const postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost)
      .mockImplementationOnce(() => mockPost);

    const login = render(<Login />);

    // email
    act(() => {
      const emailElement = screen.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = screen.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = screen.queryByTestId("login-content");
    act(() => {
      loginContent?.querySelector("form")?.submit();
    });

    await screen.findByTestId("login-error");
    expect(useFetchSpy).toBeCalled();
    expect(postSpy).toBeCalled();

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });

  it("login flow fail by backend (423)", async () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    const useFetchSpy = jest
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
    const postSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPost);

    const login = render(<Login />);

    // email
    act(() => {
      const emailElement = screen.queryByTestId("email") as HTMLInputElement;
      fireEvent.change(emailElement, { target: { value: "dont@mail.me" } });
    });

    // password
    act(() => {
      const passwordElement = screen.queryByTestId(
        "password"
      ) as HTMLInputElement;
      fireEvent.change(passwordElement, { target: { value: "password" } });
    });

    // submit
    const loginContent = screen.queryByTestId("login-content");
    act(() => {
      loginContent?.querySelector("form")?.submit();
    });

    // expect(login.html().search('class="content--error-true"')).toBeTruthy();
    // expect(login.queryByTestId("login-error")).toBeTruthy();
    await screen.findByTestId("login-error");
    expect(useFetchSpy).toBeCalled();
    expect(postSpy).toBeCalled();

    act(() => {
      Router.navigate("/");
      login.unmount();
    });
  });
});
