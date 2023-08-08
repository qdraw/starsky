import MenuDefault from "./menu-default";

export default {
  title: "components/organisms/menu-default"
};

export const Default = () => {
  return <MenuDefault isEnabled={true}></MenuDefault>;
};

Default.story = {
  name: "default"
};
