import { storiesOf } from "@storybook/react";
import React from "react";
import DetailViewGpx from './detail-view-gpx';

storiesOf("components/organisms/detail-view-gpx", module)
  .add("default", () => {
    return <>Todo: no results<br /><DetailViewGpx></DetailViewGpx></>
  })