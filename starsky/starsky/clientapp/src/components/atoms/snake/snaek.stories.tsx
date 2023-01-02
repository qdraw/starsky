import { storiesOf } from "@storybook/react";
import SnakeGame from "./snake";

storiesOf("components/atoms/snake", module).add("default", () => {
  return <SnakeGame />;
});
