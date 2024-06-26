import FormControl from "./form-control";

export default {
  title: "components/atoms/form-control"
};

export const Default = () => {
  return (
    <FormControl contentEditable={true} onBlur={() => {}} name="test">
      &nbsp;
    </FormControl>
  );
};

Default.storyName = "default";
