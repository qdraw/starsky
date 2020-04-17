import { mount, shallow } from 'enzyme';
import React from 'react';
import Notification, { NotificationType } from './notification';
import { PortalId } from './portal';

describe("ItemListView", () => {

  it("renders (without state component)", () => {
    shallow(<Notification type={NotificationType.default} />)
  });

  describe("with Context", () => {
    it("Render component", () => {
      var component = mount(<Notification type={NotificationType.default} />);
      expect(component.exists('.content')).toBeTruthy();
      component.unmount();
    });

    it("Ok close and remove element from DOM", () => {
      var component = mount(<Notification type={NotificationType.default} />);

      component.find('.icon--close').simulate('click');

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined()
    });

    it("Callback test Ok close and remove element from DOM", () => {
      var callback = jest.fn();
      var component = mount(<Notification callback={callback} type={NotificationType.default} />);

      component.find('.icon--close').simulate('click');

      expect(document.getElementById(PortalId)?.innerHTML).toBeUndefined();

      expect(callback).toBeCalled();
    });

  });
});