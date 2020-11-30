import { storiesOf } from "@storybook/react";
import React from "react";
import CurrentLocationButton from './current-location-button';

storiesOf("components/atoms/current-location-button", module)
  .add("default", () => {
    return <><CurrentLocationButton callback={(result) => { alert(`${result.latitude} ${result.longitude}`) }}></CurrentLocationButton></>
  })