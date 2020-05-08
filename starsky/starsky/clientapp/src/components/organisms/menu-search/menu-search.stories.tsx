import { storiesOf } from "@storybook/react";
import React from "react";
import MenuSearch from './menu-search';

storiesOf("components/organisms/menu-search", module)
  .add("default", () => {
    return <MenuSearch />
  })