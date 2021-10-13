import { act, render } from "@testing-library/react";
import React from "react";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import * as FetchGet from "../shared/fetch-get";
import * as FetchPost from "../shared/fetch-post";
import { UrlQuery } from "../shared/url-query";
import AccountRegister from "./account-register";

describe("AccountRegister", () => {
  it("renders", () => {
    render(<AccountRegister />);
  });

  it("link to TOC exist", () => {
    var compontent = render(<AccountRegister />);
    expect(compontent.queryByTestId("toc")).toBeTruthy();
  });

  it("link to privacy exist", () => {
    var compontent = render(<AccountRegister />);
    expect(compontent.queryByTestId("privacy")).toBeTruthy();
  });

  it("not allowed get 403 from api", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 403,
        data: null
      } as IConnectionDefault
    );
    var fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const email = container.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeTruthy();

    const password = container.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeTruthy();

    const confirmPassword = container.getByTestId(
      "confirm-password"
    ) as HTMLInputElement;
    expect(confirmPassword.disabled).toBeTruthy();

    expect(fetchGetSpy).toBeCalled();

    container.unmount();
  });

  it("allowed get 200 from api", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    var fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });
    console.log(container.container.innerHTML);

    const email = container.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeFalsy();

    const password = container.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeFalsy();

    const confirmPassword = container.getByTestId(
      "confirm-password"
    ) as HTMLInputElement;
    expect(confirmPassword.disabled).toBeFalsy();

    expect(fetchGetSpy).toBeCalled();

    container.unmount();
  });

  it("allowed get 202 from api, it should hide sign in instead button", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 202,
        data: null
      } as IConnectionDefault
    );

    var fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const email = container.getByTestId("email") as HTMLInputElement;
    expect(email.disabled).toBeTruthy();

    const password = container.getByTestId("password") as HTMLInputElement;
    expect(password.disabled).toBeTruthy();

    const confirmPassword = container.getByTestId(
      "confirm-password"
    ) as HTMLInputElement;
    expect(confirmPassword.disabled).toBeTruthy();

    const signInInstead = container.getByTestId("sign-in-instead");

    expect(signInInstead).toBeTruthy();

    expect(signInInstead.classList).toContain("disabled");

    expect(fetchGetSpy).toBeCalled();

    container.unmount();
  });

  it("submit no content", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    const button = container.queryByTestId(
      "account-register-submit"
    ) as HTMLButtonElement;

    act(() => {
      button.click();
    });

    const error = container.queryByTestId(
      "account-register-error"
    ) as HTMLElement;

    expect(error).toBeTruthy();
  });

  it("submit short password", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    await act(async () => {
      (container
        .find('[name="email"]')
        .getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      container.find('[name="email"]').simulate("change");
      (container
        .find('[name="password"]')
        .getDOMNode() as HTMLInputElement).value = "123";
      container.find('[name="password"]').simulate("change");
      (container
        .find('[name="confirm-password"]')
        .getDOMNode() as HTMLInputElement).value = "123";
      // await next one
      await container.find('[name="confirm-password"]').simulate("change");

      // and submit
      container.find('[type="submit"]').last().simulate("submit");
    });

    expect(container.exists(".content--error-true")).toBeTruthy();
  });

  it("submit password No Match", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    await act(async () => {
      (container
        .find('[name="email"]')
        .getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      container.find('[name="email"]').simulate("change");
      (container
        .find('[name="password"]')
        .getDOMNode() as HTMLInputElement).value = "123456789";
      container.find('[name="password"]').simulate("change");
      (container
        .find('[name="confirm-password"]')
        .getDOMNode() as HTMLInputElement).value = "123";
      // await next one
      await container.find('[name="confirm-password"]').simulate("change");

      // and submit
      container.find('[type="submit"]').last().simulate("submit");
    });

    expect(container.exists(".content--error-true")).toBeTruthy();
  });

  it("submit password Happy flow", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // use ==> import * as FetchPost from '../shared/fetch-post';
    const mockPostIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: "Ok"
      } as IConnectionDefault
    );

    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPostIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    await act(async () => {
      (container
        .find('[name="email"]')
        .getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      container.find('[name="email"]').simulate("change");
      (container
        .find('[name="password"]')
        .getDOMNode() as HTMLInputElement).value = "987654321";
      container.find('[name="password"]').simulate("change");
      (container
        .find('[name="confirm-password"]')
        .getDOMNode() as HTMLInputElement).value = "987654321";
      // await next one
      await container.find('[name="confirm-password"]').simulate("change");

      // and submit
      container.find('[type="submit"]').last().simulate("submit");
    });

    expect(fetchPostSpy).toBeCalled();

    expect(fetchPostSpy).toBeCalledWith(
      new UrlQuery().UrlAccountRegisterApi(),
      `Email=dont@mail.me&Password=987654321&ConfirmPassword=987654321`
    );
  });

  it("submit password antiforgery token missing (return error 400 from api)", async () => {
    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: null
      } as IConnectionDefault
    );

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    // use ==> import * as FetchPost from '../shared/fetch-post';
    const mockPostIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 400,
        data: null
      } as IConnectionDefault
    );

    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockPostIConnectionDefault);

    // need to await here
    var container = render(<></>);
    await act(async () => {
      container = await render(<AccountRegister />);
    });

    await act(async () => {
      (container
        .find('[name="email"]')
        .getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      container.find('[name="email"]').simulate("change");
      (container
        .find('[name="password"]')
        .getDOMNode() as HTMLInputElement).value = "987654321";
      container.find('[name="password"]').simulate("change");
      (container
        .find('[name="confirm-password"]')
        .getDOMNode() as HTMLInputElement).value = "987654321";
      // await next one
      await container.find('[name="confirm-password"]').simulate("change");

      // and submit
      container.find('[type="submit"]').last().simulate("submit");
    });

    expect(fetchPostSpy).toBeCalled();

    // search for .html()
    expect(container.html().indexOf("content--error-true") >= 1).toBeTruthy();
  });
});
