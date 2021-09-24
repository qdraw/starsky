import { render } from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IHealthEntry } from "../../../interfaces/IHealthEntry";
import * as Notification from "../../atoms/notification/notification";
import HealthStatusError from "./health-status-error";

describe("HealthStatusError", () => {
  it("renders (without state component)", () => {
    render(<HealthStatusError />);
  });

  describe("with Context", () => {
    it("Ok", () => {
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });
      var component = render(<HealthStatusError />);

      expect(component.html()).toBe(null);
    });

    it("Error 500", () => {
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return { ...newIConnectionDefault(), statusCode: 500 };
      });

      // usage => import * as Notification from './notification';
      var notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => {
          return null;
        });

      var component = render(<HealthStatusError>t</HealthStatusError>);

      expect(notificationSpy).toBeCalled();

      // cleanup afterwards
      notificationSpy.mockClear();
      component.unmount();
    });

    it("Error 500 with content", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return {
          ...newIConnectionDefault(),
          statusCode: 500,
          data: {
            entries: [
              {
                isHealthy: false,
                name: "ServiceNameUnhealthy"
              },
              {
                isHealthy: true,
                name: "ServiceNameIsHealthy"
              }
            ] as IHealthEntry[]
          }
        };
      });

      // usage => import * as Notification from './notification';
      var notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => {
          return null;
        });

      var component = render(<HealthStatusError />);

      expect(notificationSpy).toBeCalled();

      // cleanup afterwards
      notificationSpy.mockClear();
      component.unmount();
    });
  });
});
