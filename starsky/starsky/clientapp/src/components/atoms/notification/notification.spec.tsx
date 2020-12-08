import { mount, shallow } from "enzyme";
import React from "react";
import { PortalId } from "../portal/portal";
import Notification, { NotificationType } from "./notification";

describe("ItemListView", () => {
  it("renders (without state component)", () => {
    shallow(<Notification type={NotificationType.default} />);
  });

  describe("with Context", () => {
    it("Render component", () => {
      var component = mount(<Notification type={NotificationType.default} />);
      expect(component.exists(".content")).toBeTruthy();
      component.unmount();
    });

    it("Ok close and remove element from DOM", () => {
      var component = mount(<Notification type={NotificationType.default} />);

      component.find(".icon--close").simulate("click");

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined();
    });

    it("Portal is already gone", () => {
      var component = mount(
        <Notification type={NotificationType.default}>test</Notification>
      );

      var portalElement = document.getElementById(PortalId);
      if (!portalElement) throw new Error("portal should not be undefined");
      portalElement.remove();

      component.find(".icon--close").simulate("click");

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined();
    });

    it("Callback test Ok close", () => {
      var callback = jest.fn();
      var component = mount(
        <Notification callback={callback} type={NotificationType.default}>
          test
        </Notification>
      );

      component.find(".icon--close").simulate("click");

      // entire portal is removed from DOM

      expect(callback).toBeCalled();
    });

    it("Multiple notification it should remove them all", () => {
      mount(
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
