import { storiesOf } from "@storybook/react";
import React from "react";
import Breadcrumb from './breadcrumbs';

/**
 * subPath is child folder
 * Breadcrumb variable should only contain parent Folders
 */
storiesOf("components/molecules/breadcrumbs", module)
  .add("default", () => {
    var breadcrumbs = ["/", "/test"];
    return <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
  })