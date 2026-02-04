import MenuInlineSearch from "./menu-inline-search";

export default {
  title: "components/molecules/menu-inline-search"
};

export const Default = () => {
  return (
    <>
      Refactor: remove header-tag{" "}
      <div className="header">
        <MenuInlineSearch />
      </div>
    </>
  );
};

Default.storyName = "default";
