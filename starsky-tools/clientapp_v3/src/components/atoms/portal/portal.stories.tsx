import { storiesOf } from "@storybook/react";
import React from "react";
import Portal from './portal';

storiesOf("components/atoms/portal", module)
  .add("default", () => {
    return <Portal>should be outside the DOM</Portal>
  })