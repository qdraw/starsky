import ColorClassSelect from "./color-class-select";

export default {
  title: "components/molecules/color-class-select"
};

export const Default = () => {
  return (
    <ColorClassSelect
      collections={true}
      isEnabled={true}
      filePath={"/test"}
      onToggle={() => {}}
    />
  );
};

Default.story = {
  name: "default"
};
