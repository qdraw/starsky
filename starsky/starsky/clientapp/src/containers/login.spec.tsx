import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFetch from '../hooks/use-fetch';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import Login from './login';

describe("Login", () => {
  it("renders", () => {
    shallow(<Login />)
  });

  it("account already logged in", () => {
    globalHistory.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 200, data: 'true' } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    var login = mount(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(new UrlQuery().UrlAccountStatus(), 'get');
    expect(login.exists('.content--error-true')).toBeTruthy();
    expect(login.exists('.content--header')).toBeTruthy();

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account already logged in special return url", () => {
    globalHistory.navigate("/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 200, data: 'true' } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    var login = mount(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(login.exists('[data-test="logout"]')).toBeTruthy();
    expect(login.exists('[data-test="stayLoggedin"]')).toBeTruthy();

    // no prefix
    expect(login.find('[data-test="logout"]').props().href).toBe("/account/logout?ReturnUrl=/test");
    expect(login.find('[data-test="stayLoggedin"]').props().href).toBe("/test");

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account logged in /starsky - return url", () => {
    globalHistory.navigate("/starsky/?ReturnUrl=/test");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 200, data: 'true' } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    var login = mount(<Login />);

    expect(useFetchSpy).toBeCalled();
    expect(login.exists('[data-test="logout"]')).toBeTruthy();
    expect(login.exists('[data-test="stayLoggedin"]')).toBeTruthy();

    // including starsky prefix
    expect(login.find('[data-test="logout"]').props().href).toBe("/starsky/account/logout?ReturnUrl=/starsky/test");
    expect(login.find('[data-test="stayLoggedin"]').props().href).toBe("/starsky/test");

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

  it("account not logged in", () => {
    globalHistory.navigate("/?ReturnUrl=/");

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    var login = mount(<Login />);

    expect(login.find(".form-control").length).toBe(2);
    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(new UrlQuery().UrlAccountStatus(), 'get');

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });


  it("account 406 UrlAccountRegister", () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 406 } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    var login = mount(<Login />);

    expect(globalHistory.location.pathname.indexOf(new UrlQuery().UrlAccountRegister())).toBeTruthy();
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

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 200, data: 'ok' });
    var postSpy = jest.spyOn(FetchPost, 'default')
      .mockImplementationOnce(() => mockPost)

    var login = mount(<Login />);

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      (login.find('input[type="email"]').getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      login.find('input[type="email"]').first().simulate('change');
      (login.find('input[type="password"]').getDOMNode() as HTMLInputElement).value = "password";
      login.find('input[type="password"]').first().simulate('change');
    });

    act(() => {
      login.find('form [type="submit"]').first().simulate('submit');
    });

    expect(login.find(".form-control").length).toBe(2);
    expect(useFetchSpy).toBeCalled();
    expect(useFetchSpy).toBeCalledWith(new UrlQuery().UrlAccountStatus(), 'get');

    expect(postSpy).toBeCalled();
    expect(postSpy).toBeCalledWith(new UrlQuery().UrlLoginPage(), "Email=dont@mail.me&Password=password");

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });


  it("login flow fail by backend", () => {

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const connectionDefaultExample = { statusCode: 401 } as IConnectionDefault;

    var useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)
      .mockImplementationOnce(() => connectionDefaultExample)

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 401, data: 'fail' });
    var postSpy = jest.spyOn(FetchPost, 'default')
      .mockImplementationOnce(() => mockPost)

    var login = mount(<Login />);

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      (login.find('input[type="email"]').getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      login.find('input[type="email"]').first().simulate('change');
    });

    act(() => {
      (login.find('input[type="password"]').getDOMNode() as HTMLInputElement).value = "password";
      login.find('input[type="password"]').first().simulate('change');
    });

    act(() => {
      login.find('form [type="submit"]').first().simulate('submit');
    });

    expect(login.html().search('class="content--error-true"')).toBeTruthy();
    expect(useFetchSpy).toBeCalled();
    expect(postSpy).toBeCalled();

    act(() => {
      globalHistory.navigate("/");
      login.unmount();
    });
  });

});