import Select from "./select";

export default {
  title: "components/atoms/select"
};

export const Default = () => {
  const list: string[] = ["Saab", "Audi", "Mercedes"];
  return <Select selectOptions={list} />;
};

Default.storyName = "default";

export const SelectedAudi = () => {
  const list: string[] = ["Saab", "Audi", "Mercedes"];
  return <Select selected={"Audi"} selectOptions={list} />;
};

SelectedAudi.storyName = "selected audi";
