import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as AppContext from '../contexts/archive-context';
import { newIArchive } from '../interfaces/IArchive';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import { Query } from '../shared/query';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';

describe("ArchiveSidebarColorClass", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />)
  });

  describe("mount object (mount= select is child element)", () => {
    var wrapper = mount(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />);

    it("colorclass--select class exist", () => {
      expect(wrapper.exists('.colorclass--select')).toBeTruthy()
    });

    it("not disabled", () => {
      expect(wrapper.exists('.disabled')).toBeFalsy()
    });

    it("FAIL ==== test click", () => {

      // use this: ==> import * as AppContext from '../contexts/archive-context';
      const contextValues = {
        state: newIArchive(),
        dispatch: jest.fn(),
      } as AppContext.IArchiveContext;

      jest
        .spyOn(AppContext, 'useArchiveContext')
        .mockImplementation(() => contextValues);

      jest.mock('@reach/router', () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn(),
      }))

      // dit faalt gewoon 100%
      // jest.mock('./color-class-select', () => () => <div />);

      // Fake child element
      // const mockColorClassSelect = <div />
      // jest
      //   .spyOn(ColorClassSelect)
      //   .mockReturnValue(mockColorClassSelect);
      // // .mockImplementation(() => mockColorClassSelect);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test.jpg");
      });

      // spy on fetch
      const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve(newIFileIndexItemArray());
      var spy = jest.spyOn(Query.prototype, 'queryUpdateApi').mockImplementationOnce(() => mockFetchAsXml);


      // act(() => {
      // const TestComponent = () => (
      //   <ArchiveContextProvider>
      //     <ArchiveSidebarColorClass isReadOnly={true} fileIndexItems={newIFileIndexItemArray()} />
      //   </ArchiveContextProvider>
      // );
      // });


      const element = mount(<ArchiveSidebarColorClass isReadOnly={false} fileIndexItems={newIFileIndexItemArray()} />);



      // Make sure that the element exist in the first place
      expect(element.find('a.colorclass--1')).toBeTruthy();


      element.find('a.colorclass--1').simulate("click");
      console.log(element.html());




      // expect(contextValues.dispatch).toHaveBeenCalled();



      // expect(contextValues.dispatch).toHaveBeenCalled();

      // // var dom: HTMLElement = element.find('a.colorclass--1').getDOMNode();
      // // dom.click();

      // console.log(dom.classList);

      // throw Error();

    });


  });

  // it("not disabled2", () => {

  //   const TestComponent = () => (
  //     <ArchiveContextProvider>
  //       <ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />
  //     </ArchiveContextProvider>
  //   );
  //   const element = shallow(<TestComponent />);
  //   var find = element.find('a.colorclass--1')
  //   console.log(find.html());

  //   expect(element.find(ArchiveSidebarColorClass).dive().text()).toBe("Provided Value");

  //   // wrapper.find('a.colorclass--1').simulate('click');

  // });

});

// https://kevsoft.net/2019/05/28/testing-custom-react-hooks.html