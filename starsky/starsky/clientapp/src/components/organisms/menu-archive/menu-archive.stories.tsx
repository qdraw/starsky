import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import MenuArchive from './menu-archive';

storiesOf("components/organisms/menu-archive", module)
  .add("default", () => {
    globalHistory.navigate("/");
    return <MenuArchive></MenuArchive>
  })
  .add("select", () => {
    globalHistory.navigate("/?select=true");
    return <MenuArchive></MenuArchive>
  })