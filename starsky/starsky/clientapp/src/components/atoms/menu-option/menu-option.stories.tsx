import MenuOption from "./menu-option";

export default {
  title: "components/atoms/menu-option"
};

export const Default = () => {
  return (
    <MenuOption
      localization={{ nl: "Nederlands", en: "English" }}
      isSet={false}
      set={() => {}}
      testName="test"
      isReadOnly={false}
      setEnableMoreMenu={(value) => {
        alert(value);
      }}
    />
  );
};

Default.story = {
  name: "default"
};
