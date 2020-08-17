import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import DetailViewGpx from './detail-view-gpx';

storiesOf("components/organisms/detail-view-gpx", module)
  .add("default", () => {
    globalHistory.navigate("/?f=/test.gpx");
    return <div className="detailview"><DetailViewGpx></DetailViewGpx></div>
  })