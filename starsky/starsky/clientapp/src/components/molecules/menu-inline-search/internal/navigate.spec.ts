import { ILocationObject } from "../../../../hooks/use-location/interfaces/ILocationObject";
import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { NavigateFunction } from "../../../../hooks/use-location/type/NavigateFunction";
import { UrlQuery } from "../../../../shared/url/url-query";
import Navigate from "./navigate";

describe("Navigate", () => {
  const history: IUseLocation = {
    navigate: jest.fn() as unknown as NavigateFunction,
    location: {} as ILocationObject
  };
  const setFormFocus = jest.fn();
  const inputFormControlReference = { current: { value: "" } } as React.RefObject<HTMLInputElement>;
  const query = "test query";
  const callback = jest.fn();

  it("navigates to search page with query", () => {
    Navigate(history, setFormFocus, inputFormControlReference, query);
    expect(history.navigate).toHaveBeenCalledWith(new UrlQuery().UrlSearchPage(query));
  });

  it("updates input field value with query", () => {
    Navigate(history, setFormFocus, inputFormControlReference, query);
    expect(inputFormControlReference.current?.value).toBe(query);
  });

  it("calls callback function with query", () => {
    Navigate(history, setFormFocus, inputFormControlReference, query, callback);
    expect(callback).toHaveBeenCalledWith(query);
  });

  it("sets form focus to false", () => {
    Navigate(history, setFormFocus, inputFormControlReference, query);
    expect(setFormFocus).toHaveBeenCalledWith(false);
  });
});
