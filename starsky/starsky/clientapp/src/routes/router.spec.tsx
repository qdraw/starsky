import { globalHistory } from '@reach/router';
import { mount } from 'enzyme';
import React from 'react';
import * as ContentPage from '../pages/content-page';
import * as ImportPage from '../pages/import-page';
import * as LoginPage from '../pages/login-page';
import * as NotFoundPage from '../pages/not-found-page';
import * as SearchPage from '../pages/search-page';
import * as TrashPage from '../pages/trash-page';
import RouterApp from './router';

describe("Router", () => {
  it("default", () => {
    var contentPageSpy = jest.spyOn(ContentPage, 'default').mockImplementationOnce(() => { return <></> });
    mount(<RouterApp></RouterApp>);
    expect(contentPageSpy).toBeCalled();
  });

  it("search", () => {
    var searchPagePageSpy = jest.spyOn(SearchPage, 'default').mockImplementationOnce(() => { return <></> });
    globalHistory.navigate("/search?q=t");
    mount(<RouterApp></RouterApp>);
    expect(searchPagePageSpy).toBeCalled();
  });

  it("TrashPage", () => {
    var trashPagePageSpy = jest.spyOn(TrashPage, 'default').mockImplementationOnce(() => { return <></> });
    globalHistory.navigate("/trash?q=t");
    mount(<RouterApp></RouterApp>);
    expect(trashPagePageSpy).toBeCalled();
  });

  it("ImportPage", () => {
    var importPagePageSpy = jest.spyOn(ImportPage, 'default').mockImplementationOnce(() => { return <></> });
    globalHistory.navigate("/import?q=t");
    mount(<RouterApp></RouterApp>);
    expect(importPagePageSpy).toBeCalled();
  });

  it("LoginPage", () => {
    var loginPagePageSpy = jest.spyOn(LoginPage, 'default').mockImplementationOnce(() => { return <></> });
    globalHistory.navigate("/account/login");
    mount(<RouterApp></RouterApp>);
    expect(loginPagePageSpy).toBeCalled();
  });

  it("NotFoundPage", () => {
    var notFoundPageSpy = jest.spyOn(NotFoundPage, 'default').mockImplementationOnce(() => { return <></> });
    globalHistory.navigate("/not-found");
    mount(<RouterApp></RouterApp>);
    expect(notFoundPageSpy).toBeCalled();
  });

});