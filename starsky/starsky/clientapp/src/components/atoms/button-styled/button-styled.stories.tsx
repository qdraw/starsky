import { Router } from "../../../router-app/router-app";
import ButtonStyled from "./button-styled";

export default {
  title: "components/atoms/button-styled"
};

export const Default = () => {
  Router.navigate("/?select=test.jpg");
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
