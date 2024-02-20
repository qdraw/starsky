import MenuOptionModal from "./menu-option-modal.tsx";

export default {
  title: "components/atoms/menu-option-modal"
};

export const Default = () => {
  return (
    <div className="menu-context">
      <ul className="menu-options">
        <MenuOptionModal
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

Default.storyName = "default";
