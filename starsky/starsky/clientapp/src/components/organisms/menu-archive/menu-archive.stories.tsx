import { globalHistory } from "@reach/router";
import MenuArchive from "./menu-archive";

export default {
  title: "components/organisms/menu-archive"
};

export const Default = () => {
  globalHistory.navigate("/");
  return <MenuArchive></MenuArchive>;
};

Default.story = {
  name: "default"
};

export const Select = () => {
  globalHistory.navigate("/?select=true");
  return <MenuArchive></MenuArchive>;
};

Select.story = {
  name: "select"
};
