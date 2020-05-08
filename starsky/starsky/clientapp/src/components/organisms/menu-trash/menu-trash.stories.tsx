import { storiesOf } from "@storybook/react";
import React from "react";
import MenuTrash from './menu-trash';

storiesOf("components/organisms/menu-trash", module)
  .add("default", () => {
    return <MenuTrash />
  })