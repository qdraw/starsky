import { FunctionComponent, useState } from "react";

interface IMenuDesktopInlineSearchProps {
  callback?(query: string): void;
}

export const MenuDesktopInlineSearch: FunctionComponent<IMenuDesktopInlineSearchProps> = () => {
  // used for color of icon
  const [inputFocus, setInputFocus] = useState(true);

  return (
    <>
      <div className="menu-desktop-inline-search">
        <label
          htmlFor="menu-inline-search"
          className={inputFocus ? "icon-addon--search" : "icon-addon--search-focus"}
        >
          Search
        </label>
        <input
          id={"menu-desktop-inline-search"}
          maxLength={80}
          onBlur={() => {
            setInputFocus((name) => !name);
          }}
          onFocus={() => {
            setInputFocus((name) => !name);
          }}
          className={"form-control icon-addon--input"}
          autoComplete="off"
          data-test="menu-desktop-inline-search"
        />
      </div>
    </>
  );
};
