import { storiesOf } from "@storybook/react";
import React from "react";
import MenuDetailView from './menu-detail-view';

storiesOf("components/organisms/menu-detail-view", module)
  .add("default", () => {
    return <MenuDetailView />
  })