import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
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

      var component = render(<PreferencesUsername />);
      expect(component.find(".content--text").text()).toBe("Unknown username");
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

      var component = render(<PreferencesUsername />);
      expect(component.find(".content--text").text()).toBe("Unknown username");
      component.unmount();
    });

    it("should get the identifier", () => {
      var testReply = {
        ...newIConnectionDefault(),
        data: {
          credentialsIdentifiers: ["test"]
        },
        statusCode: 200
      };
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => testReply)
        .mockImplementationOnce(() => testReply);

      var component = render(<PreferencesUsername />);
      expect(component.find(".content--text").text()).toBe("test");
    });
  });
});
