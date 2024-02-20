import MenuSearch from "./menu-search";

export default {
  title: "components/organisms/menu-search"
};

export const Default = () => {
  return <MenuSearch state={undefined as any} dispatch={() => {}} />;
};

Default.storyName = "default";
