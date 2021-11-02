import { fireEvent, render, RenderResult } from "@testing-library/react";
import React from "react";
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
      var component = render(<PreferencesPassword />);

      act(() => {
        component.queryByTestId("preferences-password-submit")?.click();
      });

      const warning = component.queryByTestId(
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
        const passwordElement = container.queryByTestId(
          "preferences-password-input"
        ) as HTMLInputElement;
        fireEvent.change(passwordElement, { target: { value: passwordInput } });
      });

      act(() => {
        const passwordChangedElement = container.queryByTestId(
          "preferences-password-changed-input"
        ) as HTMLInputElement;
        fireEvent.change(passwordChangedElement, {
          target: { value: changedPassword }
        });
      });

      act(() => {
        const confirmPasswordElement = container.queryByTestId(
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
      const loginContent = container.queryByTestId(
        "preferences-password-submit"
      ) as HTMLButtonElement;
      act(() => {
        loginContent.click();
      });
    }

    it("The passwords do not match", () => {
      var component = render(<PreferencesPassword />);

      submitPassword(component, "12345", "password1", "something-else");

      const warning = component.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe("The passwords do not match");

      component.unmount();
    });

    it("Test if your password has been successfully changed", async () => {
      var component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../../../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 200, data: { success: true } }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      let warning = component.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).toBeNull();

      const loginContent = component.queryByTestId(
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

      const warning1 = component.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning1).not.toBeNull();

      expect(warning1.textContent).toBe(
        "Your password has been successfully changed"
      );

      component.unmount();
    });

    it("Test if enter your current password", async () => {
      var component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 401, data: null }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      const loginContent = component.queryByTestId(
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

      const warning = component.queryByTestId(
        "preferences-password-warning"
      ) as HTMLDivElement;
      expect(warning).not.toBeNull();

      expect(warning.textContent).toBe("Enter your current password");

      component.unmount();
    });

    it("Modal Error - The new password does not meet the criteria", async () => {
      var component = render(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        { statusCode: 400, data: null }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      submitPassword(component, "12345", "password1", "password1", false);

      const loginContent = component.queryByTestId(
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

      const warning = component.queryByTestId(
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
