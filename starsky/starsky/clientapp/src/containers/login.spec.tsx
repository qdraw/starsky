import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as FetchGet from '../shared/fetch-get';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import Login from './login';

describe("Login", () => {
  it("renders", () => {
    shallow(<Login />)
  });

  it("account already logged in", async () => {

    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 200 });
    var fetchGetSpy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);

    // need to await this
    await act(async () => {
      await expect(login.find(".form-control").length).toBe(2);
    });

    login.update();

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlAccountStatus());
    expect(login.exists('.content--error-true')).toBeTruthy();
    expect(login.exists('.content--header')).toBeTruthy();

    act(() => {
      login.unmount();
    });
  });

  it("account already logged in / - return url", async () => {
    globalHistory.navigate("/?ReturnUrl=/test");

    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 200 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);

    // need to await this
    await act(async () => {
      await expect(login.find(".form-control").length).toBe(2);
    });

    login.update();

    expect(login.exists('[data-test="logout"]')).toBeTruthy();
    expect(login.exists('[data-test="stayLoggedin"]')).toBeTruthy();

    // no prefix
    expect(login.find('[data-test="logout"]').props().href).toBe("/account/logout?ReturnUrl=/test");
    expect(login.find('[data-test="stayLoggedin"]').props().href).toBe("/test");

    act(() => {
      login.unmount();
      globalHistory.navigate("/");
    });
  });

  it("account logged in /starsky - return url", async () => {
    globalHistory.navigate("/starsky/?ReturnUrl=/test");

    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 200 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);

    // need to await this
    await act(async () => {
      await expect(login.find(".form-control").length).toBe(2);
    });

    login.update();

    expect(login.exists('[data-test="logout"]')).toBeTruthy();
    expect(login.exists('[data-test="stayLoggedin"]')).toBeTruthy();

    // including starsky prefix
    expect(login.find('[data-test="logout"]').props().href).toBe("/starsky/account/logout?ReturnUrl=/starsky/test");
    expect(login.find('[data-test="stayLoggedin"]').props().href).toBe("/starsky/test");

    act(() => {
      login.unmount();
      globalHistory.navigate("/");
    });
  });

  it("account not logged in", () => {
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 401 });
    var fetchGetSpy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);
    expect(login.find(".form-control").length).toBe(2);
    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlAccountStatus());

    act(() => {
      login.unmount();
      fetchGetSpy.mockClear();
    });
  });

  it("account 406 UrlAccountRegister", async () => {
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 406 });
    var fetchGetSpy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);

    // need to await this
    await act(async () => {
      await expect(login.find(".form-control").length).toBe(2);
    });

    expect(globalHistory.location.pathname.indexOf(new UrlQuery().UrlAccountRegister())).toBeTruthy();
    expect(fetchGetSpy).toBeCalled();

    act(() => {
      login.unmount();
      globalHistory.navigate("/");
    });
  });

  it("login flow succesfull", async () => {
    // Show extra information
    act(() => {
      globalHistory.navigate("/?ReturnUrl=/");
    });

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockStatus: Promise<any> = Promise.resolve({ statusCode: 401 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockStatus);

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 200, data: 'ok' });
    var postSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockPost);

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

    // need to await here
    await act(async () => {
      await login.find('form [type="submit"]').first().simulate('submit');
    });

    expect(postSpy).toBeCalled();
    expect(postSpy).toBeCalledWith(new UrlQuery().UrlLoginPage(), "Email=dont@mail.me&Password=password");

    act(() => {
      login.unmount();
    });
  });

  it("login flow fail by backend", async () => {
    // Show extra information
    act(() => {
      globalHistory.navigate("/?ReturnUrl=/");
    });

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockStatus: Promise<any> = Promise.resolve({ statusCode: 401 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockStatus);

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 401, data: 'wrong pass' });
    var postSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockPost);

    var login = mount(<Login />);

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      (login.find('input[type="email"]').getDOMNode() as HTMLInputElement).value = "dont@mail.me";
      login.find('input[type="email"]').first().simulate('change');

      (login.find('input[type="password"]').getDOMNode() as HTMLInputElement).value = "password";
      login.find('input[type="password"]').first().simulate('change');
    });

    // need to await here
    await act(async () => {
      await login.find('form [type="submit"]').first().simulate('submit');
    });

    expect(postSpy).toBeCalled();
    expect(postSpy).toBeCalledWith(new UrlQuery().UrlLoginPage(), "Email=dont@mail.me&Password=password");


    expect(login.html().search('class="content--error-true"')).toBeTruthy();

    act(() => {
      login.unmount();
    });
  });

});