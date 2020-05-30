import { storiesOf } from "@storybook/react";
import React from "react";
import PreferencesAppSettings from './preferences-app-settings';

storiesOf("components/organisms/preferences-app-settings", module)
  .add("default", () => {
    return <PreferencesAppSettings />
  })