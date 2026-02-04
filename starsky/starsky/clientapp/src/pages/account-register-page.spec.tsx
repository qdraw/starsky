import { render } from "@testing-library/react";
import * as AccountRegister from "../containers/account-register";
import { AccountRegisterPage } from "./account-register-page";

describe("ContentPage", () => {
  it("default", () => {
    const accountRegisterSpy = jest.spyOn(AccountRegister, "default").mockImplementationOnce(() => {
      return <></>;
    });
    render(<AccountRegisterPage />);
    expect(accountRegisterSpy).toHaveBeenCalledTimes(1);
  });
});
