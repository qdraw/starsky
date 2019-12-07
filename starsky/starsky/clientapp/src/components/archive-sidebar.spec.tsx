import { mount, shallow } from "enzyme";
import React from 'react';
import { PageType } from '../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import ArchiveSidebar from './archive-sidebar';

describe("ArchiveSidebar", () => {

  it("renders", () => {
    shallow(<ArchiveSidebar pageType={PageType.Loading} subPath={"/"} isReadOnly={true} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />)
  });

  describe("with mount", () => {

    beforeEach(() => {
      jest.spyOn(React, 'useLayoutEffect').mockImplementation(React.useEffect);
    });

    it("restore scroll after unmount", () => {
      var scrollTo = jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      var component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"} isReadOnly={true} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />)

      component.unmount();

      expect(scrollTo).toBeCalled();
      expect(scrollTo).toBeCalledWith(0, 0);
    });

    it("no warning if is not read only", () => {
      var component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"} isReadOnly={false} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />)
      expect(component.find('.warning-box')).toBeTruthy();
    });

    it("show warning if is read only", () => {
      var component = mount(<ArchiveSidebar pageType={PageType.Archive} subPath={"/"} isReadOnly={true} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />)
      expect(component.find('.warning-box')).toBeTruthy();
    });
  });
});