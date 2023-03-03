import {
  fireEvent,
  render,
  RenderResult,
  screen
} from "@testing-library/react";
import { act } from "react-dom/test-utils";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import PreferencesPassword from "./preferences-password";

describe("PreferencesPassword", () => {
  it("renders", () => {
    render(<PreferencesPassword />);
  });

  describe("context", () => {
    it("default nothing entered", () => {
      const component = render(<PreferencesPassword />);

      act(() => {
        screen.getByTestId("preferences-password-submit")?.click();
      });

      const warning = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe("Enter the current and new password");

      component.unmount();
    });

    function submitPassword(
      container: RenderResult,
      passwordInput: string,
      changedPassword: string,
      confirmPassword: string,
      submit: boolean = true
    ) {
      act(() => {
        const passwordElement = screen.queryByTestId(
          "preferences-password-input"
        ) as HTMLInputElement;
        fireEvent.change(passwordElement, { target: { value: passwordInput } });
      });

      act(() => {
        const passwordChangedElement = screen.queryByTestId(
          "preferences-password-changed-input"
        ) as HTMLInputElement;
        fireEvent.change(passwordChangedElement, {
          target: { value: changedPassword }
        });
      });

      act(() => {
        const confirmPasswordElement = screen.queryByTestId(
          "preferences-password-changed-confirm-input"
        ) as HTMLInputElement;
        fireEvent.change(confirmPasswordElement, {
          target: { value: confirmPassword }
        });
      });

      if (!submit) {
        return;
      }

      // submit
      const loginContent = screen.queryByTestId(
        "preferences-password-submit"
      ) as HTMLButtonElement;
      act(() => {
        loginContent.click();
      });
    }

    it("The passwords do not match", () => {
      const component = render(<PreferencesPassword />);

      submitPassword(component, "12345", "password1", "something-else");

      const warning = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe("The passwords do not match");

      component.unmount();
    });

    it("Test if your password has been successfully changed", async () => {
      const component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../../../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200, data: { success: true } });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      let warning = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).toBeNull();

      const loginContent = screen.queryByTestId(
        "preferences-password-submit"
      ) as HTMLButtonElement;

      // need await here
      await act(async () => {
        await loginContent.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1"
      );

      const warning1 = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning1).not.toBeNull();

      expect(warning1.textContent).toBe(
        "Your password has been successfully changed"
      );

      component.unmount();
    });

    it("Test if enter your current password", async () => {
      const component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 401, data: null });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      const loginContent = screen.queryByTestId(
        "preferences-password-submit"
      ) as HTMLButtonElement;

      // need await here
      await act(async () => {
        await loginContent.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1"
      );

      const warning = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe("Enter your current password");

      component.unmount();
    });

    it("Modal Error - The new password does not meet the criteria", async () => {
      const component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 400, data: null });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      const loginContent = screen.queryByTestId(
        "preferences-password-submit"
      ) as HTMLButtonElement;

      // need await here
      await act(async () => {
        await loginContent.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1"
      );

      const warning = screen.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe(
        "The new password does not meet the criteria"
      );

      component.unmount();
    });
  });
});
