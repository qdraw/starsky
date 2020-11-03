import { storiesOf } from "@storybook/react";
import React from "react";
import HealthStatusError from '../health-status-error/health-status-error';
import HealthCheckForUpdates from './health-check-for-updates';

storiesOf("components/molecules/health-check-for-updates", module)
  .add("default", () => {
    return <><b>There nothing shown yet, only if the api returns a error code</b><HealthStatusError /><HealthCheckForUpdates /></>
  })
  .add("Electron", () => {
    (window as any).isElectron = true
    return <><b>There nothing shown yet, only if the api returns a error code</b><HealthStatusError /><HealthCheckForUpdates /></>
  })
