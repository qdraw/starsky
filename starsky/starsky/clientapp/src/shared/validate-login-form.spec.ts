import { validateLoginForm } from "./validate-login-form";

describe("validateLoginForm", () => {
  it("default", () => {
    const result = validateLoginForm("", "");
    expect(result).toBeFalsy();
  });

  it("wrong email", () => {
    const result = validateLoginForm("test", "test");
    expect(result).toBeNull();
  });

  it("good", () => {
    const result = validateLoginForm("test@test.nl", "test");
    expect(result).toBeTruthy();
  });
});
