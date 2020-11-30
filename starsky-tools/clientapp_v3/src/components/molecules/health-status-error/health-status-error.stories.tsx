import { storiesOf } from "@storybook/react";
import React from "react";
import HealthStatusError from './health-status-error';

storiesOf("components/molecules/health-status-error", module)
  .add("default", () => {
    return <><b>There nothing shown yet, only if the api returns a error code</b><HealthStatusError /></>
  })