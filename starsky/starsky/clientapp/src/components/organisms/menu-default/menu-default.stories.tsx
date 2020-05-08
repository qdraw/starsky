import { storiesOf } from "@storybook/react";
import React from "react";
import MenuDefault from './menu-default';

storiesOf("components/organisms/menu-default", module)
  .add("default", () => {
    return <MenuDefault isEnabled={true}></MenuDefault>
  })