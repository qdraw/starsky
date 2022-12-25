import { storiesOf } from "@storybook/react";
import { Preferences } from "./preferences";

storiesOf("containers/preferences", module).add("default", () => {
  return <Preferences />;
});
