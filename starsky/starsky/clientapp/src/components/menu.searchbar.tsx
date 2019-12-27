import * as React from "react";
import { memo, useEffect } from 'react';
import useFetch from '../hooks/use-fetch';
import useLocation from '../hooks/use-location';

interface IMenuSearchBarProps {
  defaultText?: string;
  callback?(query: string): void;
}

const MenuSearchBar: React.FunctionComponent<IMenuSearchBarProps> = memo((props) => {
  var defaultMenu = [
    { "name": "Home", "url": "/" },
    { "name": "Foto's van deze week", "url": "/search?t=-Datetime%3E7%20-ImageFormat-%22tiff%22" },
    { "name": "Prullenmand", url: "/trash" },
    { "name": "Importeren", url: "/import" }
  ];
  var history = useLocation();

  // the results
  const [data, setData] = React.useState({
    hits: new Array<string>()
  });

  // to store the search query
  const [query, setQuery] = React.useState(props.defaultText ? props.defaultText : "");
  useEffect(() => {
    if (props.defaultText === query) return;
    setQuery(props.defaultText ? props.defaultText : "");
  }, [props]);

  // used for color of icon
  const [inputFocus, setInputFocus] = React.useState(true);

  // can't set this inside effect or if ==> performance issue, runs to often
  const responseObject = useFetch("/suggest/?t=" + query, 'get');

  useEffect(() => {
    if (!responseObject.data) return;
    var result: Array<string> = responseObject.data;
    setData({
      hits: result,
    })
  }, [responseObject]);

  function onFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    navigate(query);
  }

  function navigate(defQuery: string) {
    // setLoading(true);
    // To do change to search page
    history.navigate("/search?t=" + defQuery);
    setFormFocus(false);

    if (!props.callback) return;
    props.callback(defQuery);
  }

  const [formFocus, setFormFocus] = React.useState(false);

  useEffect(() => {
    // Bind the event listener
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      // Unbind the event listener on clean up
      document.removeEventListener("mousedown", handleClickOutside);
    };
  });


  function handleClickOutside(event: MouseEvent) {
    var target = event.target as HTMLElement;
    if (target.className.indexOf("menu-item") === -1 && target.className.indexOf("icon-addon") === -1 && target.className.indexOf("search-icon") === -1) {
      setFormFocus(false);
    }
  }

  return (
    <>
      {/* {isLoading ? <Preloader isOverlay={false} isDetailMenu={false}></Preloader> : null} */}

      <div className={!formFocus ? "blur" : ""} onFocus={() => setFormFocus(true)}>
        <li className="menu-item menu-item--half-extra">
          <form className="form-inline form-nav icon-addon" onSubmit={onFormSubmit}>

            <label htmlFor="search" className={inputFocus ? "icon-addon--search" : "icon-addon--search-focus"} />
            <input className={"form-control icon-addon--input"}
              onBlur={() => { setInputFocus(!inputFocus) }} onFocus={() => { setInputFocus(!inputFocus) }}
              autoComplete="off" value={query} onChange={e => { setQuery(e.target.value); }} />
          </form>
        </li>
        {data.hits && data.hits.length === 0 ?
          defaultMenu.map((value, index) => {
            return <li className="menu-item menu-item--default" key={index}><a href={value.url}>{value.name}</a> </li>;
          }) : null
        }
        {data.hits && data.hits.map(item => (
          <li key={item} className="menu-item menu-item--results">
            <button onClick={() => navigate(item)} className="search-icon">{item}</button>
          </li>
        ))}
      </div>
    </>
  );

});

export default MenuSearchBar;


