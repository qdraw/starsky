import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useLocation from '../hooks/use-location';
import * as FetchGet from '../shared/fetch-get';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import Login from './login';

describe("Login", () => {
  it("renders", () => {
    shallow(<Login />)
  });

  it("not logged in", () => {
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 401 });
    var spy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var login = mount(<Login />);
    expect(login.find(".form-control").length).toBe(2);
    expect(spy).toBeCalled();
    expect(spy).toBeCalledWith(new UrlQuery().UrlAccountStatus());
  });

  it("account is logged in", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 200 });
    var spy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

    act(() => {
      var login = mount(<Login />);

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith(new UrlQuery().UrlAccountStatus());
      expect(login.exists('.content--error-true')).toBeFalsy();
      expect(login.exists('.content--header')).toBeTruthy()
    });
  });

  it("login flow succesfull", () => {
    // Show extra information
    globalHistory.navigate("/?ReturnUrl=/");

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockStatus: Promise<any> = Promise.resolve({ statusCode: 401 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockStatus);

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 200, data: 'ok' });
    var postSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockPost);

    // use as ==> import * as useLocation from '../hooks/use-location';
    var useLocationSpy = jest.spyOn(useLocation, 'default');

    var login = mount(<Login />);
    console.log(globalHistory.location.search);

    (login.find('input[type="email"]').getDOMNode() as HTMLInputElement).value = "dont@mail.me";
    login.find('input[type="email"]').first().simulate('change');

    (login.find('input[type="password"]').getDOMNode() as HTMLInputElement).value = "password";
    login.find('input[type="password"]').first().simulate('change');

    login.find('form [type="submit"]').first().simulate('submit');

    expect(postSpy).toBeCalled();
    expect(postSpy).toBeCalledWith(new UrlQuery().UrlLogin(), "Email=dont@mail.me&Password=password");

    expect(useLocationSpy).toBeCalled();
  });

  it("login flow fail", async () => {
    // Show extra information
    globalHistory.navigate("/?ReturnUrl=/");

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockStatus: Promise<any> = Promise.resolve({ statusCode: 401 });
    jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockStatus).mockImplementationOnce(() => mockStatus).mockImplementationOnce(() => mockStatus).mockImplementationOnce(() => mockStatus);

    const mockPost: Promise<any> = Promise.resolve({ statusCode: 401, data: 'wrong pass!' });
    jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockPost).mockImplementationOnce(() => mockPost);

    var login = mount(<Login />);

    (login.find('input[type="email"]').getDOMNode() as HTMLInputElement).value = "dont@mail.me";
    login.find('input[type="email"]').first().simulate('change');

    (login.find('input[type="password"]').getDOMNode() as HTMLInputElement).value = "wrong";
    login.find('input[type="password"]').first().simulate('change');

    login.find('form [type="submit"]').first().simulate('submit');

    // Do not remove this await
    var result = await login.instance();
    expect(result).toBe(null);

    expect(login.html().search('class="content--error-true"')).toBeTruthy();

  });

});