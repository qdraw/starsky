import { storiesOf } from "@storybook/react";
import MoreMenu from "./more-menu";

storiesOf("components/atoms/more-menu", module).add("default", () => {
  return <MoreMenu setEnableMoreMenu={() => {}}>test</MoreMenu>;
});
