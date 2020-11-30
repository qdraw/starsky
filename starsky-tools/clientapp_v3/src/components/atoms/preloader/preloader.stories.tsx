import { storiesOf } from "@storybook/react";
import React from "react";
import Preloader from './preloader';

storiesOf("components/atoms/preloader", module)
  .add("default", () => {
    return <Preloader isOverlay={false} />
  })