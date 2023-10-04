import MenuTrash from "./menu-trash";

export default {
  title: "components/organisms/menu-trash"
};

export const Default = () => {
  return (
    <MenuTrash state={{ fileIndexItems: [] } as any} dispatch={() => {}} />
  );
};

Default.story = {
  name: "default"
};
