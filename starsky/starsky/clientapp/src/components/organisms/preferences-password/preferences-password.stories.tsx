import { storiesOf } from "@storybook/react";
import React from "react";
import { PreferencesPassword } from './preferences-password';

storiesOf("components/organisms/preferences-password", module)
  .add("default", () => {
    return <PreferencesPassword />
  })