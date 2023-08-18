import { Router } from "../router-app/router-app";
import { Sidebar } from "./sidebar";
describe("sidebar", () => {
  let setSidebarSpy = jest.fn();
  let sidebar: Sidebar;
  let navigateSpy = jest.fn();

  beforeEach(() => {
    navigateSpy = jest.fn();
    setSidebarSpy = jest.fn();
    sidebar = new Sidebar(setSidebarSpy, {
      location: window.location,
      navigate: navigateSpy
    });
  });

  describe("toggleSidebar", () => {
    it("default", () => {
      sidebar.toggleSidebar();
      expect(setSidebarSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("?sidebar=true", { replace: true });
    });

    it("toggle two times", () => {
      Router.navigate("/test?sidebar=true");

      // invoke after update
      sidebar = new Sidebar(setSidebarSpy, {
        location: {
          href: "",
          ...Router.state.location
        },
        navigate: navigateSpy
      });

      sidebar.toggleSidebar();

      expect(setSidebarSpy).toBeCalledTimes(1);
      expect(navigateSpy).toBeCalledWith("?sidebar=false", { replace: true });
    });
  });
});
