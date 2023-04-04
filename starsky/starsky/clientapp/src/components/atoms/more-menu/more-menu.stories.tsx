import MoreMenu from "./more-menu";

export default {
  title: "components/atoms/more-menu"
};

export const Default = () => {
  return <MoreMenu setEnableMoreMenu={() => {}}>test</MoreMenu>;
};

Default.story = {
  name: "default"
};
