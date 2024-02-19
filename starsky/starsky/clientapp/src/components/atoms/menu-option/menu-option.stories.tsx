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
          testName="test"
          isReadOnly={false}
          onClickKeydown={() => {
            alert("hi");
          }}
        />
      </ul>
    </div>
  );
};

Default.storyName = "default";
