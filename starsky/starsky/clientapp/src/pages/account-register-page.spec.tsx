import { mount } from 'enzyme';
import React from 'react';
import * as AccountRegister from '../containers/account-register';
import AccountRegisterPage from './account-register-page';

describe("ContentPage", () => {
  it("default", () => {
    var accountRegisterSpy = jest.spyOn(AccountRegister, 'default').mockImplementationOnce(() => { return <></> });
    mount(<AccountRegisterPage>g</AccountRegisterPage>);
    expect(accountRegisterSpy).toBeCalledTimes(1)
  });

});