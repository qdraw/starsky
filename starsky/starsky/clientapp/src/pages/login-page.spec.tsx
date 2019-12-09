import { mount } from 'enzyme';
import React from 'react';
import * as Login from '../containers/login';
import LoginPage from './login-page';


describe("LoginPage", () => {
  it("has Login child Component", () => {
    const spyLoginComponent = jest.spyOn(Login, 'default').mockImplementationOnce(() => { return <></> });
    mount(<LoginPage></LoginPage>);
    expect(spyLoginComponent).toBeCalled();
  });

});