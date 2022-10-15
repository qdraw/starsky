import { storiesOf } from "@storybook/react";
import React from "react";
import Select from "./select";

storiesOf("components/atoms/select", module)
  .add("default", () => {
    const list: string[] = ["Saab", "Audi", "Mercedes"];
    return <Select selectOptions={list} />;
  })
  .add("selected audi", () => {
    const list: string[] = ["Saab", "Audi", "Mercedes"];
    return <Select selected={"Audi"} selectOptions={list} />;
  });
