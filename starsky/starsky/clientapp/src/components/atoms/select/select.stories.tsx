import React from "react";
import Select from "./select";

export default {
  title: "components/atoms/select"
};

export const Default = () => {
  const list: string[] = ["Saab", "Audi", "Mercedes"];
  return <Select selectOptions={list} />;
};

Default.story = {
  name: "default"
};

export const SelectedAudi = () => {
  const list: string[] = ["Saab", "Audi", "Mercedes"];
  return <Select selected={"Audi"} selectOptions={list} />;
};

SelectedAudi.story = {
  name: "selected audi"
};
