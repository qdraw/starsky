import { storiesOf } from "@storybook/react";
import React from "react";
import MenuArchive from './menu-archive';

storiesOf("components/organisms/menu-archive", module)
  .add("default", () => {
    return <MenuArchive></MenuArchive>
  })