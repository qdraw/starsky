import { storiesOf } from "@storybook/react";
import React from "react";
import DetailViewMp4 from './detail-view-mp4';

storiesOf("components/organisms/detail-view-mp4", module)
  .add("default", () => {
    return <>Todo: no results<br /><DetailViewMp4></DetailViewMp4></>
  })