import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import ButtonStyled from './button-styled';

storiesOf("components/atoms/button-styled", module)
  .add("default", () => {
    globalHistory.navigate("/?select=test.jpg");
    return <ButtonStyled className="btn btn--default" type="submit" disabled={false} onClick={e => { }}>
      Loading...
    </ButtonStyled>
  })