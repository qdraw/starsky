import { IArchiveProps } from "../interfaces/IArchiveProps";
import { Router } from "../router-app/router-app";
import { Select } from "./select";

describe("select", () => {
  describe("removeSidebarSelection", () => {
    it("single disable", () => {
      Router.navigate("/?select=");
      const setSelectSpy = jest.fn();
      const select = new Select([], setSelectSpy, {} as IArchiveProps, {
        location: {
          href: "",
          ...Router.state.location
        },
        navigate: Router.navigate
      });
      select.removeSidebarSelection();

      expect(Router.state.location.search).toBe("");
      expect(setSelectSpy).toHaveBeenCalled();
      expect(setSelectSpy).toHaveBeenCalledWith([]);
    });

    it("multiple disable", () => {
      Router.navigate("/?select=1,2");
      const setSelectSpy = jest.fn();
      const select = new Select([], setSelectSpy, {} as IArchiveProps, {
        location: {
          href: "",
          ...Router.state.location
        },
        navigate: Router.navigate
      });
      select.removeSidebarSelection();

      expect(Router.state.location.search).toBe("");
      expect(setSelectSpy).toHaveBeenCalled();
      expect(setSelectSpy).toHaveBeenCalledWith(["1", "2"]);
    });
  });
});
