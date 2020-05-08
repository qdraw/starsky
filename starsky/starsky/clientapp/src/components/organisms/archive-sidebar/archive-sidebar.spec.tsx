import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { PageType } from '../../../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ArchiveSidebar from './archive-sidebar';

describe("ArchiveSidebar", () => {

  it("renders", () => {
    shallow(<ArchiveSidebar pageType={PageType.Loading} subPath={"/"} isReadOnly={true} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />)
  });

  describe("with mount", () => {

    beforeEach(() => {
      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })
      jest.spyOn(React, 'useLayoutEffect').mockImplementation(React.useEffect);

      globalHistory.navigate("/?sidebar=true")
    });

    it("restore scroll after unmount", () => {
      var scrollTo = jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })

      const component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"}
        isReadOnly={true} colorClassUsage={[]}
        fileIndexItems={newIFileIndexItemArray()} />);

      component.unmount();

      expect(scrollTo).toBeCalled();
      expect(scrollTo).toBeCalledWith(0, -0);
    });

    it("no warning if is not read only", () => {
      var component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"}
        isReadOnly={false} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />);
      expect(component.find('.warning-box')).toBeTruthy();
    });

    it("show warning if is read only", () => {
      const component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"}
        isReadOnly={true} colorClassUsage={[]}
        fileIndexItems={newIFileIndexItemArray()} />);
      expect(component.find('.warning-box')).toBeTruthy();
    });

    it("scroll event and set body style with scroll", () => {

      mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"}
        isReadOnly={false} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />);

      Object.defineProperty(window, 'scrollY', { value: 1 });
      window.dispatchEvent(new Event('scroll'));

      expect(document.body.style.top).toBe("-1px")

      // reset
      Object.defineProperty(window, 'scrollY', { value: 0 });
      document.body.style.top = "";
    });

    it("scroll event and set body style no scroll ", () => {

      mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"}
        isReadOnly={false} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />);

      window.dispatchEvent(new Event('scroll'));
      expect(document.body.style.top).toBe("")

    });


  });
});
