import { useEffect, useRef, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useLocation from "../../../hooks/use-location/use-location";
import { UrlQuery } from "../../../shared/url-query";
import ArrowKeyDown from "./shared/arrow-key-down";
import InlineSearchSuggest from "./shared/inline-search-suggest";
import Navigate from "./shared/navigate";

interface IMenuSearchBarProps {
  defaultText?: string;
  callback?(query: string): void;
}

const MenuInlineSearch: React.FunctionComponent<IMenuSearchBarProps> = (
  props
) => {
  const history = useLocation();

  // the results
  const [suggest, setSuggest] = useState(new Array<string>());

  // to store the search query
  const [query, setQuery] = useState(
    props.defaultText ? props.defaultText : ""
  );

  // When pressing enter within the same page
  const inputFormControlReference = useRef<HTMLInputElement>(null);

  // used for color of icon
  const [inputFocus, setInputFocus] = useState(true);

  // can't set this inside effect or if ==> performance issue, runs to often
  const responseObject = useFetch(
    new UrlQuery().UrlSearchSuggestApi(query),
    "get"
  );
  useEffect(() => {
    if (!responseObject?.data?.length || responseObject.statusCode !== 200) {
      if (suggest && suggest.length >= 1) setSuggest([]);
      return;
    }
    const result: Array<string> = [...responseObject.data];
    setSuggest(result);

    // to avoid endless loops
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [responseObject]);

  /** Submit the form */
  function onFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    Navigate(
      history,
      setFormFocus,
      inputFormControlReference,
      query,
      props.callback
    );
  }

  const featuresResult = useFetch(
    new UrlQuery().UrlApiFeaturesAppSettings(),
    "get"
  );

  /**
   * is form active
   */
  const [formFocus, setFormFocus] = useState(false);

  /**
   * Add listener to checks if you don't point outside the form
   */
  useEffect(() => {
    // Bind the event listener
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      // Unbind the event listener on clean up
      document.removeEventListener("mousedown", handleClickOutside);
    };
  });

  /**
   * If check to know if you clicked inside the form and suggestions
   * @param event mouse event from document
   */
  function handleClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (
      target.className.indexOf("menu-item") === -1 &&
      target.className.indexOf("icon-addon") === -1 &&
      target.className.indexOf("search-icon") === -1
    ) {
      setFormFocus(false);
    }
  }
  const [keyDownIndex, setKeyDownIndex] = useState(-1);

  return (
    <div className="menu-inline-search">
      <div
        className={!formFocus ? "blur" : ""}
        onFocus={() => setFormFocus(true)}
        role="presentation"
      >
        <ul>
          <li className="menu-item menu-item--half-extra">
            <form
              className="form-inline form-nav icon-addon"
              onSubmit={onFormSubmit}
            >
              <label
                htmlFor="menu-inline-search"
                className={
                  inputFocus ? "icon-addon--search" : "icon-addon--search-focus"
                }
              >
                Search
              </label>
              <input
                id={"menu-inline-search"}
                maxLength={80}
                className={"form-control icon-addon--input"}
                onBlur={() => {
                  setInputFocus((name) => !name);
                }}
                onFocus={() => {
                  setInputFocus((name) => !name);
                }}
                autoComplete="off"
                data-test="menu-inline-search"
                defaultValue={query}
                ref={inputFormControlReference}
                onKeyDown={(e) => {
                  setInputFocus(false);
                  ArrowKeyDown(
                    e,
                    keyDownIndex,
                    setKeyDownIndex,
                    inputFormControlReference.current,
                    suggest
                  );
                }}
                onChange={(e) => {
                  setQuery(e.target.value);
                }}
              />
            </form>
          </li>

          <InlineSearchSuggest
            suggest={suggest}
            defaultText={props.defaultText}
            setFormFocus={setFormFocus}
            inputFormControlReference={inputFormControlReference}
            callback={props.callback}
            featuresResult={featuresResult}
          />
        </ul>
      </div>
    </div>
  );
};
export default MenuInlineSearch;
