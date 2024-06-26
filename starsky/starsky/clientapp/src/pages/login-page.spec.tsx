import { render } from "@testing-library/react";
import * as Login from "../containers/login";
import { LoginPage } from "./login-page";

describe("LoginPage", () => {
  it("has Login child Component", () => {
    const spyLoginComponent = jest.spyOn(Login, "Login").mockImplementationOnce(() => {
      return <></>;
    });
    render(<LoginPage />);
    expect(spyLoginComponent).toHaveBeenCalled();
  });
});
