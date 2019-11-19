import { mount, shallow } from "enzyme";
import React from 'react';
import * as FetchGet from '../shared/fetch-get';
import { UrlQuery } from '../shared/url-query';
import Login from './login';

describe("DetailView", () => {
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

  // it("logged in", () => {
  //   // spy on fetch
  //   // use this import => import * as FetchPost from '../shared/fetch-post';
  //   const mockFetchAsXml: Promise<any> = Promise.resolve({ statusCode: 401 });
  //   var spy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockFetchAsXml);

  //   act(() => {
  //     var login = mount(<Login />);

  //     expect(spy).toBeCalled();
  //     console.log(login.html());


  //     console.log(login.find('.content--error-true').length);
  //   });


  //   expect(spy).toBeCalledWith();

  // });


});