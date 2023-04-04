import { globalHistory } from "@reach/router";
import React from "react";
import DetailViewGpx from "./detail-view-gpx";

export default {
  title: "components/organisms/detail-view-gpx"
};

export const Default = () => {
  globalHistory.navigate("/?f=/test.gpx");
  return (
    <div className="detailview">
      <DetailViewGpx></DetailViewGpx>
    </div>
  );
};

Default.story = {
  name: "default"
};
