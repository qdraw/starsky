import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFetch from '../hooks/use-fetch';
import { IArchive } from '../interfaces/IArchive';
import { newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import MenuArchive from './menu-archive';
import * as ModalArchiveMkdir from './modal-archive-mkdir';

describe("MenuArchive", () => {

  it("renders", () => {
    shallow(<MenuArchive />)
  });

  describe("with Context", () => {

    beforeEach(() => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })
        .mockImplementationOnce(() => { })
    });

    it("default menu", () => {

      globalHistory.navigate("/");

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="hamburger"]')).toBeTruthy();
      expect(component.exists('.item--select')).toBeTruthy();
      expect(component.exists('.item--more')).toBeTruthy();

      // and clean
      component.unmount();
    });

    it("none selected", () => {

      globalHistory.navigate("?select=");

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

      component.unmount();
    });

    it("two selected", () => {

      globalHistory.navigate("?select=test1,test2");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="selected-2"]')).toBeTruthy();

      component.unmount();
    });


    it("menu click mkdir", () => {

      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      var mkdirModalSpy = jest.spyOn(ModalArchiveMkdir, 'default').mockImplementationOnce(() => {
        return <></>
      })

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="mkdir"]');

      act(() => {
        item.simulate('click');
      });

      expect(mkdirModalSpy).toBeCalled();

      component.unmount();

    });
  });

});

