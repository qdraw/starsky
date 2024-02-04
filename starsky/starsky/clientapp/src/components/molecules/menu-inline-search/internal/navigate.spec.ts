import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { UrlQuery } from "../../../../shared/url-query";
import Navigate from "./navigate";

describe("Navigate", () => {
  const history: IUseLocation = {
    navigate: jest.fn() as any,
    location: {} as any
  };
  const setFormFocus = jest.fn();
  const inputFormControlReference = { current: { value: "" } } as any;
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
