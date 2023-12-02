import MenuOption from "./menu-option";

export default {
  title: "components/atoms/menu-option"
};

export const Default = () => {
  return (
    <div className="menu-context">
      <ul className="menu-options">
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
      </ul>
    </div>
  );
};

Default.story = {
  name: "default"
};
