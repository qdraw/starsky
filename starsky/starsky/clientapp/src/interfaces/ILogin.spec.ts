import { ILogin, newILogin } from "./ILogin";

describe("ILogin", () => {
  it("newILogin", () => {
    const login = newILogin();
    expect(login).toStrictEqual({} as ILogin);
  });
});
