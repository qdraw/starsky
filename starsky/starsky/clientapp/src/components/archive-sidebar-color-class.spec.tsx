import { mount, shallow } from "enzyme";
import React from 'react';
import { ArchiveContextProvider } from '../contexts/archive-context';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
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

    // it("not d2222isabled", () => {

    //   var a = wrapper.find('a.colorclass--1'); // .simulate('click');

    //   a.first().simulate('click'); // does nothing
    // });



    // it("not22122222 disabled", () => {
    //   const app = shallow(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />);
    //   const onButtonClickSpy = jest.spyOn(app.instance(), "dispatch");

    //   // # This should do the trick
    //   app.update();
    //   app.instance().forceUpdate();

    //   const button = app.find("button");
    //   button.simulate("click");
    //   expect(onButtonClickSpy).toHaveBeenCalled();
    // });


    it("not222 disabled", () => {

      //   const app = shallow(<App />);
      //   const onButtonClickSpy = jest.spyOn(app.instance(), "onButtonClick");

      //   // # This should do the trick
      // app.update();
      //   app.instance().forceUpdate();

      //   const button = app.find("button");
      //   button.simulate("click");
      //   expect(onButtonClickSpy).toHaveBeenCalled();

      const setState = jest.fn();
      const useStateSpy = jest.spyOn(React, 'useContext')
      useStateSpy.mockImplementation((init) => [init, setState]);

      const TestComponent = () => (
        <ArchiveContextProvider>
          <ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />
        </ArchiveContextProvider>
      );

      const element = mount(<TestComponent />);

      expect(element.find('a.colorclass--1')).toBeTruthy()

      var dom: HTMLElement = element.find('a.colorclass--1').getDOMNode();
      // console.log(dom);

      dom.click();
      // console.log(element.find('a.colorclass--1').html());

      // Fake news
      // var a = element.find('a.colorclass--1'); // .simulate('click');

      // a.simulate('click');

      expect(useStateSpy).toBeCalledTimes(2)


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