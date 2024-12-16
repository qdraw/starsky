import { act, fireEvent, render, RenderResult, screen, waitFor } from "@testing-library/react";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import * as FetchGet from "../shared/fetch/fetch-get";
import * as FetchPost from "../shared/fetch/fetch-post";
import { UrlQuery } from "../shared/url/url-query";
import AccountRegister from "./account-register";

describe("AccountRegister", () => {
  it("renders", () => {
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(
        () => Promise.resolve({ statusCode: 4638 }) as Promise<IConnectionDefault>
      );
    render(<AccountRegister />);
  });

  it("link to TOC exist", () => {
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(
        () => Promise.resolve({ statusCode: 876 }) as Promise<IConnectionDefault>
      );
    const component = render(<AccountRegister />);
    expect(screen.getByTestId("toc")).toBeTruthy();

    component.unmount();
  });

  it("link to privacy exist", () => {
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(
        () => Promise.resolve({ statusCode: 123 }) as Promise<IConnectionDefault>
      );
    const component = render(<AccountRegister />);
    expect(component.getByTestId("privacy")).toBeTruthy();

    component.unmount();
  });

  it("not allowed get 403 from api", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 403,
      data: null
    } as IConnectionDefault);
    jest.spyOn(FetchGet, "default").mockReset();
    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const email = screen.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeTruthy();

    const password = screen.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeTruthy();

    const confirmPassword = screen.getByTestId("confirm-password") as HTMLInputElement;
    expect(confirmPassword.disabled).toBeTruthy();

    expect(fetchGetSpy).toHaveBeenCalled();

    container.unmount();
  });

  it("allowed get 200 from api", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const email = screen.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeFalsy();

    const password = screen.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeFalsy();

    const confirmPassword = screen.getByTestId("confirm-password") as HTMLInputElement;
    expect(confirmPassword.disabled).toBeFalsy();

    expect(fetchGetSpy).toHaveBeenCalled();

    container.unmount();
  });

  it("allowed get 202 from api, it should hide sign in instead button", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 202,
      data: null
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const email = screen.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeFalsy();

    const password = screen.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeFalsy();

    const confirmPassword = screen.getByTestId("confirm-password") as HTMLInputElement;
    expect(confirmPassword.disabled).toBeFalsy();

    const signInInstead = screen.getByTestId("sign-in-instead");

    expect(signInInstead).toBeTruthy();

    expect(signInInstead.classList).toContain("disabled");

    expect(fetchGetSpy).toHaveBeenCalled();

    container.unmount();
  });

  it("submit no content", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const button = screen.queryByTestId("account-register-submit") as HTMLButtonElement;

    act(() => {
      button.click();
    });

    const error = screen.queryByTestId("account-register-error") as HTMLElement;
    expect(error).toBeTruthy();
    container.unmount();
  });

  function submitEmailPassword(
    _container: RenderResult,
    email: string,
    password: string,
    confirmPassword: string,
    submit: boolean = true
  ) {
    // email
    const emailElement = screen.queryByTestId("email") as HTMLInputElement;
    fireEvent.change(emailElement, { target: { value: email } });

    // password
    const passwordElement = screen.queryByTestId("password") as HTMLInputElement;
    fireEvent.change(passwordElement, { target: { value: password } });

    // confirm-password
    const confirmPasswordElement = screen.queryByTestId("confirm-password") as HTMLInputElement;
    fireEvent.change(confirmPasswordElement, {
      target: { value: confirmPassword }
    });

    if (!submit) {
      return;
    }

    // submit
    const loginContent = screen.queryByTestId("account-register-form") as HTMLFormElement;
    act(() => {
      loginContent.submit();
    });
  }

  it("submit short password", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    submitEmailPassword(container, "dont@mail.me", "123", "123");

    const error = screen.queryByTestId("account-register-error") as HTMLElement;
    expect(error).toBeTruthy();
  });

  it("submit password No Match", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    submitEmailPassword(container, "dont@mail.me", "123456789", "123");

    const error = screen.queryByTestId("account-register-error") as HTMLElement;
    expect(error).toBeTruthy();
  });

  it("submit password Happy flow", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockGetIConnectionDefault);

    // use ==> import * as FetchPost from '../shared/fetch-post';
    const mockPostIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: "Ok"
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPostIConnectionDefault);

    // need to await here
    let container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    submitEmailPassword(container, "dont@mail.me", "987654321", "987654321");

    expect(fetchPostSpy).toHaveBeenCalled();

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlAccountRegisterApi(),
      `Email=dont@mail.me&Password=987654321&ConfirmPassword=987654321`
    );
  });

  it("submit password antiforgery token missing (return error 400 from api)", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault)

      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // use ==> import * as FetchPost from '../shared/fetch-post';
    const mockPostIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 400,
      data: null
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockClear()
      .mockImplementationOnce(() => mockPostIConnectionDefault)
      .mockImplementationOnce(() => mockPostIConnectionDefault);

    // need to await here
    const container = render(<AccountRegister />);

    submitEmailPassword(container, "dont@mail.me", "987654321", "987654321", false);

    // submit & await
    const loginContent = screen.queryByTestId("account-register-form") as HTMLFormElement;

    // need to await
    await act(async () => {
      await loginContent.submit();
    });

    expect(fetchPostSpy).toHaveBeenCalled();
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);

    const error = screen.queryByTestId("account-register-error") as HTMLElement;

    console.log(container.container.innerHTML);

    expect(error).toBeTruthy();
  });

  it("displays an error message when the response data is falsy", async () => {
    // use ==> import * as FetchPost from '../shared/fetch-post';
    const mockPostIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: null
    } as IConnectionDefault);

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockClear()
      .mockImplementationOnce(() => mockPostIConnectionDefault)
      .mockImplementationOnce(() => mockPostIConnectionDefault);
    jest.spyOn(FetchGet, "default").mockImplementationOnce(() => mockPostIConnectionDefault);

    const component = render(<AccountRegister />);

    const emailInput = screen.getByTestId("email");
    const passwordInput = screen.getByTestId("password");
    const confirmPasswordInput = screen.getByTestId("confirm-password");
    const submitButton = screen.getByTestId("account-register-submit");

    // Act
    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "password" } });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    });

    expect(fetchPostSpy).toHaveBeenCalledWith(
      expect.any(String),
      "Email=test@example.com&Password=password&ConfirmPassword=password"
    );

    await waitFor(() => {
      const errorMessage = screen.getByTestId("account-register-error");
      expect(errorMessage).toBeTruthy();
    });
    component.unmount();
  });
});
