import {SelectMenuItem} from "./select-menu-item";

export default {
  title: "components/organisms/menu-archive/internal/select-menu-item"
};

export const Default = () => {
  return <SelectMenuItem select={undefined} removeSidebarSelection={() => {
  }} toggleLabels={() => {
  }}></SelectMenuItem>;
};

Default.storyName = "default";

export const Select = () => {
  return <SelectMenuItem select={[""]} removeSidebarSelection={() => {
  }} toggleLabels={() => {
  }}></SelectMenuItem>;
};

Select.storyName = "select";
