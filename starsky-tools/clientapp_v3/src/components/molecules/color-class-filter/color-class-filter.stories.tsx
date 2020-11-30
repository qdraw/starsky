import { storiesOf } from "@storybook/react";
import React from "react";
import ColorClassFilter from './color-class-filter';

storiesOf("components/molecules/color-class-filter", module)
  .add("default", () => {
    return <ColorClassFilter itemsCount={1} subPath={"/test"} colorClassActiveList={[1, 2, 3, 4, 5, 6, 7, 8]} colorClassUsage={[1, 2, 3, 4, 5, 6, 7, 8]}></ColorClassFilter>
  })