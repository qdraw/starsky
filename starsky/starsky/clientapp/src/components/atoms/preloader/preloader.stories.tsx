import Preloader from "./preloader";

export default {
  title: "components/atoms/preloader"
};

export const Default = () => {
  return <Preloader isOverlay={false} />;
};

Default.story = {
  name: "default"
};
