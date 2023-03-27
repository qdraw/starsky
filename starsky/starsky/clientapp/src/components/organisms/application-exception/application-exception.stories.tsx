import { storiesOf } from "@storybook/react";
import ApplicationException from "./application-exception";

storiesOf("components/organisms/application-exception", module).add(
  "default",
  () => {
    return <ApplicationException></ApplicationException>;
  }
);
