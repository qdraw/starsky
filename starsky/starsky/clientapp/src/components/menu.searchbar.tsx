import * as React from "react";
import { useEffect } from 'react';
import useFetch from '../hooks/use-fetch';

export function MenuSearchBar() {
  var defaultMenu = [
    { "name": "Home", "url": "/" },
    { "name": "Foto's van deze week", "url": "/search?t=-Datetime%3E7%20-ImageFormat-%22tiff%22" },
    { "name": "Account", url: "/account" },
    { "name": "Importeren", url: "/import" }
  ];

  // the results
  const [data, setData] = React.useState({
    hits: new Array<string>()
  });

  // to store the search query
  const [query, setQuery] = React.useState("");

  // used for color of icon
  const [focus, setFocus] = React.useState(true);

  // can't set this inside effect or if ==> performance issue, runs to often
  const responseObject = useFetch("/suggest/?t=" + query, 'get');


  useEffect(() => {
    if (!responseObject) return;
    var result: Array<string> = responseObject;
    setData({
      hits: result,
    })
  }, [responseObject]);

  function onFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    // To do change to search page
    document.location.href = "/search?t=" + query;
  }

  /**
   * To change te color of the icon to blue
   * @param event input event
   */
  function toggleBlur(event: React.FocusEvent<HTMLInputElement>) {
    setFocus(!focus);
  }

  return (
    <>
      <li className="menu-item menu-item--half-extra">
        <form className="form-inline form-nav icon-addon" onSubmit={onFormSubmit}>
          <label htmlFor="search" className={focus ? "icon-addon--search" : "icon-addon--search-focus"}></label>
          <input autoFocus className={"form-control icon-addon--input"}
            onBlur={toggleBlur} onFocus={toggleBlur}
            autoComplete="off" value={query} onChange={e => setQuery(e.target.value)} />
        </form>
      </li>
      {data.hits.length === 0 ?
        defaultMenu.map((value, index) => {
          return <li className="menu-item" key={index}><a href={value.url}>{value.name}</a> </li>;
        }) : null
      }
      {data.hits.map(item => (
        <li key={item} className="menu-item">
          <a href={"/search?t=" + item} className="search-icon">{item}</a>
        </li>
      ))}
    </>
  );

}



