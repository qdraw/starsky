import { validateLoginForm } from './validate-login-form';


describe("validateLoginForm", () => {
  it("default", () => {
    var test = jest.fn();
    validateLoginForm("", "", test)
    expect(test).toBeCalled();
    expect(test).toBeCalledWith("Voer een emailadres en een wachtwoord in");
  });

  it("wrong email", () => {
    var test = jest.fn();
    validateLoginForm("test", "test", test)
    expect(test).toBeCalled();
    expect(test).toBeCalledWith("Controleer je email adres");
  });

  it("good", () => {
    var test = jest.fn();
    validateLoginForm("test@test.nl", "test", test)
    expect(test).toBeTruthy()
  });
});