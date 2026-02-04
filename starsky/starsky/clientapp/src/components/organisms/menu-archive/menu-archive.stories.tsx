import { Router } from "../../../router-app/router-app";
import MenuArchive from "./menu-archive";
export default {
  title: "components/organisms/menu-archive"
};

export const Default = () => {
  Router.navigate("/");
  return <MenuArchive></MenuArchive>;
};

Default.storyName = "default";

export const Select = () => {
  Router.navigate("/?select=true");
  return <MenuArchive></MenuArchive>;
};

Select.storyName = "select";
