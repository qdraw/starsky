import { storiesOf } from "@storybook/react";
import React from "react";
import MenuInlineSearch from './menu-inline-search';

storiesOf("components/molecules/menu-inline-search", module)
  .add("default", () => {
    return <>Refactor: remove header-tag <div className="header"><MenuInlineSearch /></div></>
  })
