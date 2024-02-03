import SwitchButton from "./switch-button";

export default {
  title: "components/atoms/switch-button"
};

export const Default = () => {
  return <SwitchButton onToggle={() => {}} leftLabel={"on"} rightLabel={"off"} />;
};

Default.story = {
  name: "default"
};
