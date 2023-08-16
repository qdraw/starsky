import MenuArchive from "./menu-archive";
;

export default {
  title: "components/organisms/menu-archive"
};

export const Default = () => {
  window.location.replace("/");
  return <MenuArchive></MenuArchive>;
};

Default.story = {
  name: "default"
};

export const Select = () => {
  window.location.replace("/?select=true");
  return <MenuArchive></MenuArchive>;
};

Select.story = {
  name: "select"
};
