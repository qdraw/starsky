import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import * as useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import * as Notification from "../../atoms/notification/notification";
import MenuOptionDesktopEditorOpenSelectionNoSelectWarning from "./menu-option-desktop-editor-open-selection-no-select-warning";

describe("MenuOptionDesktopEditorOpenSelectionNoSelectWarning", () => {
  it("should render without crashing", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);
  });

  it("should show error notification when trying to open editor without selecting anything", () => {
    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce((event) => {
      return <p data-test="notification-spy">{event.children}</p>;
    });

    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);

    fireEvent.keyDown(window, { key: "e", ctrlKey: true });

    waitFor(() => {
      expect(notificationSpy).toHaveBeenCalledTimes(1);

      expect(screen.queryByTestId("notification-spy")).toBeTruthy();
    });
  });

  it("should not show error notification when select is not empty", () => {
    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce((event) => {
      return <p data-test="notification-spy">{event.children}</p>;
    });

    render(
      <MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={["item"]} isReadOnly={false} />
    );
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(notificationSpy).toHaveBeenCalledTimes(0);

    expect(screen.queryByTestId("notification-spy")).toBeFalsy();
  });

  it("should not show error notification when read-only mode is enabled", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={true} />);

    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce((event) => {
      return <p data-test="notification-spy">{event.children}</p>;
    });

    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(notificationSpy).toHaveBeenCalledTimes(0);

    expect(screen.queryByTestId("notification-spy")).toBeFalsy();
  });

  it("should not show error notification when editor feature is disabled", () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: false
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce((event) => {
      return <p data-test="notification-spy">{event.children}</p>;
    });

    const component = render(
      <MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />
    );

    fireEvent.keyDown(component.container, { key: "e", ctrlKey: true });

    expect(screen.queryByTestId("notification-spy")).toBeFalsy();

    expect(notificationSpy).toHaveBeenCalledTimes(0);

    expect(useFetchSpy).toHaveBeenCalled();
  });

  it("give error message", () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: true
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce((event) => {
      return <p data-test="notification-spy">{event.children}</p>;
    });

    const useHotKeysSpy = jest
      .spyOn(useHotKeys, "default")
      .mockReset()
      .mockImplementationOnce((event, callback) => {
        if (event?.key == "e" && callback) {
          callback({} as KeyboardEvent);
        }
      });

    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageItemSelectionRequired = language.key(localization.MessageItemSelectionAdvised);

    const component = render(
      <MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />
    );

    fireEvent.keyDown(component.container, { key: "e", ctrlKey: true });

    expect(screen.queryByTestId("notification-spy")).toBeTruthy();

    const errorMessage = screen.queryByTestId("notification-spy")?.innerHTML;

    expect(errorMessage).toBe(MessageItemSelectionRequired);

    expect(screen.queryByTestId("notification-spy")?.innerHTML).toBeTruthy();

    expect(notificationSpy).toHaveBeenCalledTimes(1);
    expect(useHotKeysSpy).toHaveBeenCalledTimes(2);

    expect(useFetchSpy).toHaveBeenCalled();
  });

  it("give error message and click away", () => {
    const mockGetIConnectionDefaultFeatureToggle = {
      statusCode: 200,
      data: {
        openEditorEnabled: true
      } as IEnvFeatures
    } as IConnectionDefault;

    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle)
      .mockImplementationOnce(() => mockGetIConnectionDefaultFeatureToggle);

    const notificationSpy = jest
      .spyOn(Notification, "default")
      .mockReset()
      .mockImplementationOnce((event) => {
        return (
          <>
            <p data-test="notification-spy">{event.children}</p>
            <button data-test="notification-spy-button" onClick={event.callback}></button>
          </>
        );
      });

    const useHotKeysSpy = jest
      .spyOn(useHotKeys, "default")
      .mockReset()
      .mockImplementationOnce((event, callback) => {
        if (event?.key == "e" && callback) {
          callback({} as KeyboardEvent);
        }
      });

    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageItemSelectionRequired = language.key(localization.MessageItemSelectionAdvised);

    const component = render(
      <MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />
    );

    fireEvent.keyDown(component.container, { key: "e", ctrlKey: true });

    expect(screen.queryByTestId("notification-spy")).toBeTruthy();

    const errorMessage = screen.queryByTestId("notification-spy")?.innerHTML;

    expect(errorMessage).toBe(MessageItemSelectionRequired);

    expect(screen.queryByTestId("notification-spy")?.innerHTML).toBeTruthy();

    expect(notificationSpy).toHaveBeenCalledTimes(1);
    expect(useHotKeysSpy).toHaveBeenCalledTimes(2);

    expect(useFetchSpy).toHaveBeenCalled();

    fireEvent.click(screen.getByTestId("notification-spy-button"));

    const errorMessage2 = screen.queryByTestId("notification-spy")?.innerHTML;

    expect(errorMessage2).toBe(undefined);
  });
});
