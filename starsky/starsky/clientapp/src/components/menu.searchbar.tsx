import * as React from "react";
import { memo, useEffect, useRef } from 'react';
import useFetch from '../hooks/use-fetch';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';

interface IMenuSearchBarProps {
  defaultText?: string;
  callback?(query: string): void;
}

const MenuSearchBar: React.FunctionComponent<IMenuSearchBarProps> = memo((props) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  var defaultMenu = [
    { "name": "Home", "url": new UrlQuery().UrlHomePage() },
    { "name": language.text("Foto's van deze week", "Photos of this week"), "url": new UrlQuery().UrlSearchPage('-Datetime%3E7%20-ImageFormat-%22tiff%22') },
    { "name": language.text("Prullenmand", "Trash"), url: new UrlQuery().UrlTrashPage() },
    { "name": language.text("Importeren", "Import"), url: "/import" },
    { "name": language.text("Uitloggen", "Logout"), url: "/account/login" }
  ];
  var history = useLocation();

  // the results
  const [suggest, setSuggest] = React.useState(new Array<string>());

  // to store the search query
  const [query, setQuery] = React.useState(props.defaultText ? props.defaultText : "");

  // When pressing enter within the same page
  const inputFormControlReference = useRef<HTMLInputElement>(null);

  // used for color of icon
  const [inputFocus, setInputFocus] = React.useState(true);

  // can't set this inside effect or if ==> performance issue, runs to often
  const responseObject = useFetch(new UrlQuery().UrlSearchSuggestApi(query), 'get');
  useEffect(() => {
    if (!responseObject.data) return;
    if (!responseObject.data.length) return;

    var result: Array<string> = [...responseObject.data];
    setSuggest(result)
  }, [responseObject]);

  /** Submit the form */
  function onFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    navigate(query);
  }

  /** Go to different searchpage */
  function navigate(defQuery: string) {

    // To do change to search page
    history.navigate(new UrlQuery().UrlSearchPage(defQuery));
    setFormFocus(false);

    // force update input field after navigate to page (only the input item)
    (inputFormControlReference.current as HTMLInputElement).value = defQuery;

    if (!props.callback) return;
    props.callback(defQuery);
  }

  /** 
   * is form active
   */
  const [formFocus, setFormFocus] = React.useState(false);

  /**
   * Add listener to checks if you dont point outside the form
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
    var target = event.target as HTMLElement;
    if (target.className.indexOf("menu-item") === -1 && target.className.indexOf("icon-addon") === -1 && target.className.indexOf("search-icon") === -1) {
      setFormFocus(false);
    }
  }

  return (
    <>
      <div className={!formFocus ? "blur" : ""} onFocus={() => setFormFocus(true)}>
        <li className="menu-item menu-item--half-extra">
          <form className="form-inline form-nav icon-addon" onSubmit={onFormSubmit}>
            <label htmlFor="search" className={inputFocus ? "icon-addon--search" : "icon-addon--search-focus"} />
            <input
              maxLength={80}
              className={"form-control icon-addon--input"}
              onBlur={() => { setInputFocus(!inputFocus) }}
              onFocus={() => { setInputFocus(!inputFocus) }}
              autoComplete="off"
              defaultValue={query}
              ref={inputFormControlReference}
              onKeyDown={_ => { setInputFocus(false) }}
              onChange={e => { setQuery(e.target.value); }}
            />
          </form>
        </li>
        {suggest && suggest.length === 0 ?
          defaultMenu.map((value, index) => {
            return <li className="menu-item menu-item--default" key={index}><a href={value.url}>{value.name}</a> </li>;
          }) : null
        }
        {suggest && suggest.map((item, index) => (
          index <= 8 ? <li key={item} className="menu-item menu-item--results">
            <button onClick={() => navigate(item)} className="search-icon">{item}</button>
          </li> : null
        ))}
      </div>
    </>
  );

});

export default MenuSearchBar;


