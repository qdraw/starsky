import { render } from "@testing-library/react";
import React from "react";
import { PortalId } from "../portal/portal";
import Notification, { NotificationType } from "./notification";

describe("ItemListView", () => {
  it("renders (without state component)", () => {
    render(<Notification type={NotificationType.default} />);
  });

  describe("with Context", () => {
    it("Render component", () => {
      const component = render(
        <Notification type={NotificationType.default} />
      );

      const content = component.queryByTestId("notification-content");

      expect(content).toBeTruthy();
      component.unmount();
    });

    it("Ok close and remove element from DOM", () => {
      const component = render(
        <Notification type={NotificationType.default} />
      );

      component.queryByTestId("notification-close")?.click();

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined();
    });

    it("Portal is already gone", () => {
      const component = render(
        <Notification type={NotificationType.default}>test</Notification>
      );

      var portalElement = document.getElementById(PortalId);
      if (!portalElement) throw new Error("portal should not be undefined");
      portalElement.remove();

      component.queryByTestId("notification-close")?.click();

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined();
    });

    it("Callback test Ok close", () => {
      var callback = jest.fn();
      const component = render(
        <Notification callback={callback} type={NotificationType.default}>
          test
        </Notification>
      );

      component.queryByTestId("notification-close")?.click();

      // entire portal is removed from DOM

      expect(callback).toBeCalled();
    });

    it("Multiple notification it should remove them all", () => {
      render(
        <>
          <Notification type={NotificationType.default} />
          <Notification type={NotificationType.danger} />
        </>
      );

      var portalElement = document.getElementById(PortalId);
      if (!portalElement) throw new Error("portal should not be undefined");

      // first close default
      var closeElement = portalElement.querySelector(
        ".icon--close"
      ) as HTMLDivElement;

      closeElement.click();

      var portalElement2 = document.getElementById(PortalId);
      expect(portalElement2).toBeNull();
    });
  });
});
