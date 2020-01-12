import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../hooks/use-fetch';
import { IArchive } from '../interfaces/IArchive';
import { newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import MenuArchive from './menu-archive';

describe("MenuArchive", () => {

  it("renders", () => {
    shallow(<MenuArchive />)
  });

  describe("with Context", () => {
    it("default", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="hamburger"]')).toBeTruthy();
      expect(component.exists('.item--select')).toBeTruthy();
      expect(component.exists('.item--more')).toBeTruthy();

      // and clean
      component.unmount();
    });

    it("none selected", () => {

      globalHistory.navigate("?select=");

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="selected-0"]')).toBeTruthy();

      console.log(component.html());


    });

  });

});

