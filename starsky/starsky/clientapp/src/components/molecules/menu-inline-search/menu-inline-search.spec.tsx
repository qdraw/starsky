import { act, fireEvent, render, waitFor } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import * as ArrowKeyDown from "./arrow-key-down";
import MenuInlineSearch from "./menu-inline-search";

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
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault();
        });

      const menuBar = render(<MenuInlineSearch />);

      const input = menuBar.queryByTestId(
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

      const input = menuBar.queryByTestId(
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

    it("suggestions and click button", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
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

      const input = menuBar.queryByTestId(
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
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const input = menuBar.queryByTestId(
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
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample);

      const callback = jest.fn();
      const menuBar = render(
        <MenuInlineSearch defaultText={"tes"} callback={callback} />
      );

      const arrowKeyDownSpy = jest
        .spyOn(ArrowKeyDown, "default")
        .mockImplementationOnce(() => {});

      const input = menuBar.queryByTestId(
        "menu-inline-search"
      ) as HTMLInputElement;

      expect(input).not.toBeNull();

      fireEvent.keyDown(input, { target: { value: "test" } });

      expect(arrowKeyDownSpy).toBeCalled();

      menuBar.unmount();
    });
  });
});
