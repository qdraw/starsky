import { storiesOf } from "@storybook/react";
import React from "react";
import SwitchButton from './switch-button';

storiesOf("components/atoms/switch-button", module)
  .add("default", () => {
    return <SwitchButton onToggle={() => { }} leftLabel={"on"} rightLabel={"off"} />
  })