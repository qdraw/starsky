import { globalHistory } from '@reach/router';
import { Sidebar } from './sidebar';

describe("sidebar", () => {

  var setSidebarSpy = jest.fn();
  var sidebar: Sidebar;
  var navigateSpy = jest.fn();

  beforeEach(() => {
    navigateSpy = jest.fn();
    setSidebarSpy = jest.fn();
    sidebar = new Sidebar(true, setSidebarSpy, {
      location: globalHistory.location,
      navigate: navigateSpy,
    });
  })

  describe("toggleSidebar", () => {

    it("default", () => {
      sidebar.toggleSidebar();
      expect(setSidebarSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith('?sidebar=true', { replace: true })
    });

    it("toggle two times", () => {
      globalHistory.navigate("/test?sidebar=true");

      // invoke after update
      sidebar = new Sidebar(true, setSidebarSpy, {
        location: globalHistory.location,
        navigate: navigateSpy,
      })

      sidebar.toggleSidebar();

      expect(setSidebarSpy).toBeCalledTimes(1);
      expect(navigateSpy).toBeCalledWith('?sidebar=false', { replace: true })
    });

  });
});