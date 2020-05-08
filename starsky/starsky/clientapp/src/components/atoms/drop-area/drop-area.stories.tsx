import { storiesOf } from "@storybook/react";
import React from "react";
import DropArea from './drop-area';

storiesOf("components/atoms/drop-area", module)
  .add("default", () => {
    return <><div>Drag 'n drop file to show<br /><br /></div><DropArea enableDragAndDrop={true} enableInputButton={true} endpoint="/import" /></>
  })