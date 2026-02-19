import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { RouterProvider, createMemoryRouter } from "react-router-dom";
import * as PreferencesAppSettings from "../../components/organisms/preferences-app-settings/preferences-app-settings";
import * as PreferencesCloudImport from "../../components/organisms/preferences-cloud-import/preferences-cloud-import";
import * as PreferencesPassword from "../../components/organisms/preferences-password/preferences-password";
import * as PreferencesUsername from "../../components/organisms/preferences-username/preferences-username";
import { Preferences } from "./preferences";

const renderWithRouter = (initialPath = "/preferences") => {
  const router = createMemoryRouter([{ path: "/preferences", element: <Preferences /> }], {
    initialEntries: [initialPath]
  });

  const component = render(<RouterProvider router={router} />);
  return { component, router };
};

describe("Preferences", () => {
  it("renders", () => {
    renderWithRouter();
  });

  describe("tabs", () => {
    it("should render username tab by default", async () => {
      const preferencesUsernameSpy = jest
        .spyOn(PreferencesUsername, "default")
        .mockImplementation(() => <></>);
      const preferencesPasswordSpy = jest
        .spyOn(PreferencesPassword, "default")
        .mockImplementation(() => <></>);
      const preferencesAppSettingsSpy = jest
        .spyOn(PreferencesAppSettings, "default")
        .mockImplementation(() => <></>);
      const preferencesCloudImportSpy = jest
        .spyOn(PreferencesCloudImport, "default")
        .mockImplementation(() => <></>);

      const { component, router } = renderWithRouter();

      await waitFor(() => {
        expect(router.state.location.search).toContain("tab=username");
      });

      expect(preferencesUsernameSpy).toHaveBeenCalled();
      expect(preferencesPasswordSpy).not.toHaveBeenCalled();
      expect(preferencesAppSettingsSpy).not.toHaveBeenCalled();
      expect(preferencesCloudImportSpy).not.toHaveBeenCalled();

      component.unmount();
    });

    it("should render tab from url", () => {
      const preferencesUsernameSpy = jest
        .spyOn(PreferencesUsername, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesPasswordSpy = jest
        .spyOn(PreferencesPassword, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesAppSettingsSpy = jest
        .spyOn(PreferencesAppSettings, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesCloudImportSpy = jest
        .spyOn(PreferencesCloudImport, "default")
        .mockReset()
        .mockImplementation(() => <></>);

      const { component } = renderWithRouter("/preferences?tab=password");

      expect(preferencesUsernameSpy).not.toHaveBeenCalled();
      expect(preferencesPasswordSpy).toHaveBeenCalled();
      expect(preferencesAppSettingsSpy).not.toHaveBeenCalled();
      expect(preferencesCloudImportSpy).not.toHaveBeenCalled();

      component.unmount();
    });

    it("should render cloud tab from url", () => {
      const preferencesUsernameSpy = jest
        .spyOn(PreferencesUsername, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesPasswordSpy = jest
        .spyOn(PreferencesPassword, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesAppSettingsSpy = jest
        .spyOn(PreferencesAppSettings, "default")
        .mockReset()
        .mockImplementation(() => <></>);
      const preferencesCloudImportSpy = jest
        .spyOn(PreferencesCloudImport, "default")
        .mockReset()
        .mockImplementation(() => <></>);

      const { component } = renderWithRouter("/preferences?tab=cloud");

      expect(preferencesUsernameSpy).not.toHaveBeenCalled();
      expect(preferencesPasswordSpy).not.toHaveBeenCalled();
      expect(preferencesAppSettingsSpy).not.toHaveBeenCalled();
      expect(preferencesCloudImportSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("should update url when selecting a new tab", async () => {
      const { component, router } = renderWithRouter();

      const passwordTabButton = screen.getByTestId("preferences-tab-password");
      fireEvent.click(passwordTabButton);

      await waitFor(() => {
        expect(router.state.location.search).toContain("tab=password");
      });

      const appTabButton = screen.getByTestId("preferences-tab-app");
      fireEvent.click(appTabButton);

      await waitFor(() => {
        expect(router.state.location.search).toContain("tab=app");
      });

      const cloudTabButton = screen.getByTestId("preferences-tab-cloud");
      fireEvent.click(cloudTabButton);

      await waitFor(() => {
        expect(router.state.location.search).toContain("tab=cloud");
      });

      component.unmount();
    });
  });
});
