import Portal from "./portal";

export default {
  title: "components/atoms/portal"
};

export const Default = () => {
  return <Portal>should be outside the DOM</Portal>;
};

Default.story = {
  name: "default"
};
