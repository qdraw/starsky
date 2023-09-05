import { render, screen } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { IArchive } from "../../../interfaces/IArchive";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Router } from "../../../router-app/router-app";
import MenuSearch from "./menu-search";
describe("MenuSearch", () => {
  it("renders", () => {
    render(<MenuSearch state={undefined as any} dispatch={jest.fn()} />);
  });

  describe("with Context", () => {
    it("open hamburger menu", () => {
      const component = render(
        <MenuSearch
          state={{ fileIndexItems: [] } as any}
          dispatch={jest.fn()}
        />
      );

      let hamburger = screen.queryByTestId("hamburger") as HTMLDivElement;
      let hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger");

      act(() => {
        hamburger.click();
      });

      hamburger = screen.queryByTestId("hamburger") as HTMLDivElement;
      hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger open");

      component.unmount();
    });

    it("un select items", () => {
      Router.navigate("/?select=1");
      const component = render(
        <MenuSearch
          state={{ fileIndexItems: [] } as any}
          dispatch={jest.fn()}
        />
      );

      expect(Router.state.location.search).toBe("?select=1");

      const selected1 = screen.queryByTestId("selected-1") as HTMLDivElement;

      act(() => {
        selected1.click();
      });

      expect(window.location.search).toBe("");

      component.unmount();
      Router.navigate("/");
    });

    it("keyboard ctrl a and command a", () => {
      jest.spyOn(React, "useContext").mockRestore();

      const useHotkeysSpy = jest
        .spyOn(useHotKeys, "default")
        .mockImplementationOnce(() => {
          return { key: "a", ctrlKey: true };
        });

      const state = {
        subPath: "/",
        fileIndexItems: [
          {
            status: IExifStatus.Ok,
            filePath: "/trashed/test1.jpg",
            fileName: "test1.jpg"
          }
        ]
      } as IArchive;
      const contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const component = render(
        <MenuSearch state={undefined as any} dispatch={jest.fn()} />
      );

      expect(useHotkeysSpy).toBeCalled();
      expect(useHotkeysSpy).toBeCalledTimes(1);

      jest.spyOn(React, "useContext").mockRestore();
      component.unmount();
    });
  });
});
