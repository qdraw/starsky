import { storiesOf } from "@storybook/react";
import React from "react";
import ColorClassSelect from './color-class-select';

storiesOf("components/molecules/color-class-select", module)
  .add("default", () => {
    return <ColorClassSelect collections={true} isEnabled={true} filePath={"/test"} onToggle={() => {
    }} />
  })