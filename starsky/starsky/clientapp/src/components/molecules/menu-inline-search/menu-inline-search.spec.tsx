import { act, fireEvent, render, screen, waitFor } from "@testing-library/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import MenuInlineSearch from "./menu-inline-search";
import * as ArrowKeyDown from "./internal/arrow-key-down";
import * as InlineSearchSuggest from "./internal/inline-search-suggest";

describe("menu-inline-search", () => {
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
      expect(menuBar.container.querySelector("label")?.classList).toContain("icon-addon--search");

      const input = menuBar.queryByTestId("menu-inline-search") as HTMLInputElement;

      expect(input).not.toBeNull();

      act(() => {
        input.focus();
      });

      await waitFor(() =>
        expect(menuBar.container.querySelector("label")?.classList).toContain(
          "icon-addon--search-focus"
        )
      );

      menuBar.unmount();
    });

    it("menu searchBar - blur", async () => {
      console.log("menu searchBar - blur");

      jest.spyOn(React, "useEffect").mockReset();

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      let menuBar = render(<></>);
      act(() => {
        menuBar = render(<MenuInlineSearch />);
      });

      const input = screen.queryByTestId("menu-inline-search") as HTMLInputElement;

      expect(input).not.toBeNull();

      act(() => {
        input.focus();
      });

      expect(menuBar?.container.querySelector("label")?.classList).toContain(
        "icon-addon--search-focus"
      );

      // go to blur
      act(() => {
        input.blur();
      });

      expect(menuBar.container.querySelector("label")?.classList).toContain("icon-addon--search");

      expect(menuBar.findByTestId("menu-inline-search-search-icon")).toBeTruthy();

      menuBar.unmount();
    });
  });

  describe("with Context 2", () => {
    const suggestionsExample = {
      statusCode: 200,
      data: ["suggest1", "suggest2"]
    } as IConnectionDefault;

    const dataFeaturesExample = {
      statusCode: 200,
      data: {
        systemTrashEnabled: true,
        useLocalDesktopUi: false
      } as IEnvFeatures
    } as IConnectionDefault;

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
      const menuBar = render(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      expect(screen.getByTestId("default-menu-item-trash")).toBeTruthy();
      expect(screen.getByTestId("default-menu-item-logout")).toBeTruthy();

      menuBar.unmount();
    });

    it("suggestions and click button", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample);

      const inlineSearchSuggestSpy = jest
        .spyOn(InlineSearchSuggest, "default")
        .mockImplementationOnce((props) => {
          return (
            <>
              {props.suggest?.map((query, index) =>
                index <= 8 ? (
                  <li key={query} data-test={"menu-inline-search-suggest-" + query}></li>
                ) : null
              )}
            </>
          );
        });

      const callback = jest.fn();
      const menuBar = render(
        <MemoryRouter>
          <MenuInlineSearch defaultText={"tes"} callback={callback} />
        </MemoryRouter>
      );

      expect(inlineSearchSuggestSpy).toHaveBeenCalledTimes(1);
      expect(inlineSearchSuggestSpy).toHaveBeenCalledWith(
        {
          callback: callback,
          defaultText: "tes",
          featuresResult: { data: ["suggest1", "suggest2"], statusCode: 200 },
          inputFormControlReference: expect.any(Object),
          setFormFocus: expect.any(Function),
          suggest: []
        },
        {}
      );

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
      const menuBar = render(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      const input = screen.queryByTestId("menu-inline-search") as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.change(input, { target: { value: "test" } });
      // and change it back
      fireEvent.change(input, { target: { value: "" } });

      const results = menuBar.container.querySelector(".menu-item--default");

      expect(results).toBeTruthy();

      expect(callback).toHaveBeenCalledTimes(0);
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
      const menuBar = render(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      const arrowKeyDownSpy = jest.spyOn(ArrowKeyDown, "default").mockImplementationOnce(() => {});

      const input = screen.queryByTestId("menu-inline-search") as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.keyDown(input, { target: { value: "test" } });

      expect(arrowKeyDownSpy).toHaveBeenCalled();

      menuBar.unmount();
    });
  });
});
