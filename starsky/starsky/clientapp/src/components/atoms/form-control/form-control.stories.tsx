import { storiesOf } from "@storybook/react";
import React from "react";
import FormControl from './form-control';

storiesOf("components/atoms/form-control", module)
  .add("default", () => {
    return <FormControl contentEditable={true} onBlur={() => { }} name="test">&nbsp;</FormControl>
  })