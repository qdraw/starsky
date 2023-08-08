import { globalHistory } from "@reach/router";
import ButtonStyled from "./button-styled";

export default {
  title: "components/atoms/button-styled"
};

export const Default = () => {
  globalHistory.navigate("/?select=test.jpg");
  return (
    <ButtonStyled
      className="btn btn--default"
      type="submit"
      disabled={false}
      onClick={() => {}}
    >
      Loading...
    </ButtonStyled>
  );
};

Default.story = {
  name: "default"
};
