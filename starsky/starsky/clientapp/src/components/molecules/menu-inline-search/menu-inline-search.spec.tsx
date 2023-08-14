import {
  act,
  fireEvent,
  render,
  screen,
  waitFor
} from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import MenuInlineSearch from "./menu-inline-search";
import * as ArrowKeyDown from "./shared/arrow-key-down";

describe("Menu.SearchBar", () => {
  it("renders", () => {
    render(<MenuInlineSearch />);
  });

  describe("with Context", () => {
    it("Menu.SearchBar focus", async () => {
      //
      const successResponse: IConnectionDefault = {
        ...newIConnectionDefault(),
        statusCode: 200
      };

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => successResponse)
        .mockImplementationOnce(() => successResponse)
        .mockImplementationOnce(() => successResponse)
        .mockImplementationOnce(() => successResponse);

      const menuBar = render(<MenuInlineSearch />);

      // default
      expect(menuBar.container.querySelector("label")?.classList).toContain(
        "icon-addon--search"
      );

      const input = menuBar.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      input.focus();

      await waitFor(() =>
        expect(menuBar.container.querySelector("label")?.classList).toContain(
          "icon-addon--search-focus"
        )
      );

      menuBar.unmount();
    });

    it("menu searchbar - blur", () => {
      console.log("menu searchbar - blur");

      jest.spyOn(React, "useEffect").mockReset();

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const menuBar = render(<MenuInlineSearch />);

      const input = screen.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      act(() => {
        input.focus();
      });

      expect(menuBar.container.querySelector("label")?.classList).toContain(
        "icon-addon--search-focus"
      );

      // go to blur
      act(() => {
        input.blur();
      });

      expect(menuBar.container.querySelector("label")?.classList).toContain(
        "icon-addon--search"
      );

      menuBar.unmount();
    });

    const suggestionsExample = {
      statusCode: 200,
      data: ["suggest1", "suggest2"]
    } as IConnectionDefault;

    it("inline search suggestions", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const input = screen.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.change(input, { target: { value: "test" } });

      const results = menuBar.container.querySelectorAll(
        ".menu-item--results > button"
      );

      console.log("-text results");
      for (const result of Array.from(results)) {
        console.log(result?.textContent);
      }

      const result1 = Array.from(results).find(
        (p) => p?.textContent === "suggest1"
      )?.textContent;

      const result2 = Array.from(results).find(
        (p) => p?.textContent === "suggest2"
      )?.textContent;

      expect(result1).toBeTruthy();
      expect(result2).toBeTruthy();

      expect(callback).toBeCalledTimes(0);

      menuBar.unmount();
    });

    const dataFeaturesExample = {
      statusCode: 200,
      data: {
        systemTrashEnabled: true,
        useLocalDesktopUi: false
      } as IEnvFeatures
    } as IConnectionDefault;

    it("default menu should hide trash when system trash is enabled", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => dataFeaturesExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      expect(screen.getByTestId("default-menu-item-import")).toBeTruthy();
      expect(screen.queryByTestId("default-menu-item-trash")).toBeFalsy();
      expect(screen.getByTestId("default-menu-item-logout")).toBeTruthy();

      menuBar.unmount();
    });

    it("default menu should hide logout when uselocal desktop ui is enabled", () => {
      dataFeaturesExample.data.useLocalDesktopUi = true;
      dataFeaturesExample.data.systemTrashEnabled = false;

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => dataFeaturesExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      expect(screen.getByTestId("default-menu-item-trash")).toBeTruthy();
      expect(screen.queryByTestId("default-menu-item-logout")).toBeFalsy();

      menuBar.unmount();
    });

    it("default menu should show logout and trash in default mode", () => {
      dataFeaturesExample.data.useLocalDesktopUi = false;
      dataFeaturesExample.data.systemTrashEnabled = false;

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => dataFeaturesExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      expect(screen.getByTestId("default-menu-item-trash")).toBeTruthy();
      expect(screen.getByTestId("default-menu-item-logout")).toBeTruthy();

      menuBar.unmount();
    });

    it("suggestions and click button", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample);

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const input = screen.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.change(input, { target: { value: "test" } });

      const results = menuBar.container.querySelectorAll(
        ".menu-item--results > button"
      );

      expect(results).toHaveLength(2);
      (results[0] as HTMLButtonElement).click();

      expect(callback).toBeCalled();
      menuBar.unmount();
    });

    it("reset suggestions after change to nothing", () => {
      console.log("reset suggestions after change to nothing");
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const input = screen.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.change(input, { target: { value: "test" } });
      // and change it back
      fireEvent.change(input, { target: { value: "" } });

      const results = menuBar.container.querySelector(".menu-item--default");

      expect(results).toBeTruthy();

      expect(callback).toBeCalledTimes(0);
      menuBar.unmount();
    });

    it("ArrowKeyDown called", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const arrowKeyDownSpy = jest
        .spyOn(ArrowKeyDown, "default")
        .mockImplementationOnce(() => {});

      const input = screen.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.keyDown(input, { target: { value: "test" } });

      expect(arrowKeyDownSpy).toBeCalled();

      menuBar.unmount();
    });
  });
});
