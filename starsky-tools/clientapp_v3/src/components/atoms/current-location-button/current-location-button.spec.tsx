import { mount, shallow } from 'enzyme';
import React from 'react';
import CurrentLocationButton from './current-location-button';

describe("CurrentLocationButton", () => {

  it("renders", () => {
    shallow(<CurrentLocationButton />)
  });

  describe("context", () => {

    it("no navigator.geolocation wrong_location", () => {

      var component = mount(<CurrentLocationButton />);
      component.find('button').simulate('click');
      expect(component.find('button').hasClass('icon--wrong_location')).toBeTruthy();
    });

    it("getCurrentPosition success", () => {

      const mockGeolocation = {
        getCurrentPosition: jest.fn()
          .mockImplementationOnce((success) => Promise.resolve(success({
            coords: {
              latitude: 51.1,
              longitude: 45.3
            }
          })))
      };
      (global as any).navigator.geolocation = mockGeolocation;

      var callback = jest.fn();
      var component = mount(<CurrentLocationButton callback={callback} />);
      component.find('button').simulate('click');

      expect(callback).toBeCalled();
      expect(callback).toBeCalledWith({ "latitude": 51.1, "longitude": 45.3 });

      expect(component.find('button').hasClass('icon--location_on')).toBeTruthy();
    });

    it("getCurrentPosition success no callback", () => {

      const mockGeolocation = {
        getCurrentPosition: jest.fn()
          .mockImplementationOnce((success) => Promise.resolve(success({
            coords: {
              latitude: 51.1,
              longitude: 45.3
            }
          })))
      };
      (global as any).navigator.geolocation = mockGeolocation;

      var component = mount(<CurrentLocationButton />);
      component.find('button').simulate('click');

      // no callback
      expect(component.find('button').hasClass('icon--location_on')).toBeTruthy();
    });


    it("getCurrentPosition error", () => {

      const mockGeolocation = {
        getCurrentPosition: jest.fn()
          .mockImplementationOnce((_, error) => Promise.resolve(error()))
      };
      (global as any).navigator.geolocation = mockGeolocation;

      var callback = jest.fn();
      var component = mount(<CurrentLocationButton callback={callback} />);
      component.find('button').simulate('click');

      expect(component.find('button').hasClass('icon--wrong_location')).toBeTruthy();
    });

  });
});