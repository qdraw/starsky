import { render, screen } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import { newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import localization from "../../../localization/localization.json";
import PreferencesUsername from "./preferences-username";

describe("PreferencesUsername", () => {
  it("renders", () => {
    render(<PreferencesUsername />);
  });

  describe("context", () => {
    it("Error 500", () => {
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return { ...newIConnectionDefault(), statusCode: 500 };
      });

      const component = render(<PreferencesUsername />);
      expect(screen.getByTestId("preferences-username-text")?.textContent).toBe("Unknown username");
      component.unmount();
    });

    it("Incomplete data", () => {
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return {
          ...newIConnectionDefault(),
          data: {
            credentials: []
          },
          statusCode: 200
        };
      });

      const component = render(<PreferencesUsername />);
      expect(screen.queryByTestId("preferences-username-text")?.textContent).toBe(
        "Unknown username"
      );
      component.unmount();
    });

    it("should get the identifier", () => {
      const testReply = {
        ...newIConnectionDefault(),
        data: {
          credentialsIdentifiers: ["test"]
        },
        statusCode: 200
      };
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => testReply);

      const component = render(<PreferencesUsername />);
      expect(screen.queryByTestId("preferences-username-text")?.textContent).toBe("test");

      component.unmount();
    });

    it("should get the identifier desktop user", () => {
      const testReply = {
        ...newIConnectionDefault(),
        data: {
          credentialsIdentifiers: ["mail@localhost"]
        },
        statusCode: 200
      };
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => testReply)
        .mockImplementationOnce(() => testReply);

      const component = render(<PreferencesUsername />);
      expect(screen.queryByTestId("preferences-username-text")?.textContent).toBe(
        localization.MessageDesktopMailLocalhostUsername.en
      );

      component.unmount();
    });
  });
});
