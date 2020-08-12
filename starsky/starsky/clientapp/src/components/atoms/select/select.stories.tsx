import { storiesOf } from "@storybook/react";
import React from "react";
import Select from './select';

storiesOf("components/atoms/select", module)
  .add("default", () => {
    var list: string[] = ["Saab", "Audi", "Mercedes"]

    return <Select selectOptions={list} />
  })