import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import MenuSearch from "./menu-search";

export default {
  title: "components/organisms/menu-search"
};

export const Default = () => {
  return <MenuSearch state={undefined as unknown as IArchiveProps} dispatch={() => {}} />;
};

Default.storyName = "default";
