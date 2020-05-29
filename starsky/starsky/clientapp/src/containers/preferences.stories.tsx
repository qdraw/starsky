import { storiesOf } from "@storybook/react";
import React from "react";
import { Preferences } from './preferences';

storiesOf("containers/settings", module)
  .add("default", () => {
    return <Preferences />
  })