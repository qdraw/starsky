import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IArchive } from '../interfaces/IArchive';
import { IExifStatus } from '../interfaces/IExifStatus';
import MenuTrash from './menu-trash';

describe("MenuTrash", () => {

  it("renders", () => {
    shallow(<MenuTrash defaultText="" callback={() => { }} />)
  });

  describe("with Context", () => {

    let contextValues: any;

    beforeEach(() => {

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Deleted, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;

      contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
    });


    it("open hamburger menu", () => {
      var component = mount(<MenuTrash />)
      var hamburger = component.find('.hamburger');

      expect(component.exists('.form-nav')).toBeTruthy();
      expect(component.exists('.hamburger.open')).toBeFalsy();
      expect(component.exists('.nav.open')).toBeFalsy();

      act(() => {
        hamburger.simulate('click');
      });

      expect(component.html()).toContain('hamburger open')
      expect(component.html()).toContain('nav open')
      expect(component.exists('.form-nav')).toBeTruthy();

      component.unmount();
    });

    it("open hamburger menu", () => {


      var component = mount(<MenuTrash />)
      var hamburger = component.find('.hamburger');

      expect(component.exists('.form-nav')).toBeTruthy();
      expect(component.exists('.hamburger.open')).toBeFalsy();
      expect(component.exists('.nav.open')).toBeFalsy();

      act(() => {
        hamburger.simulate('click');
      });

      expect(component.html()).toContain('hamburger open')
      expect(component.html()).toContain('nav open')
      expect(component.exists('.form-nav')).toBeTruthy();

      component.unmount();
    });


  });
});
