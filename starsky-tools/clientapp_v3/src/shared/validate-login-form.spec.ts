import { validateLoginForm } from './validate-login-form';

describe("validateLoginForm", () => {
  it("default", () => {
    var result = validateLoginForm("", "");
    expect(result).toBeFalsy();
  });

  it("wrong email", () => {
    var result = validateLoginForm("test", "test")
    expect(result).toBeNull();
  });

  it("good", () => {
    var result = validateLoginForm("test@test.nl", "test")
    expect(result).toBeTruthy()
  });
});