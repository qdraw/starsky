import { ILogin, newILogin } from './ILogin';

describe("ILogin", () => {
  it("newILogin", () => {
    var login = newILogin();
    expect(login).toStrictEqual({} as ILogin);
  });
});