import { storiesOf } from "@storybook/react";
import React from "react";
import Notification, { NotificationType } from "./notification";

storiesOf("components/atoms/notification", module)
  .add("default", () => {
    return (
      <Notification type={NotificationType.default}>
        "There are critical errors in the following components:"
      </Notification>
    );
  })
  .add("danger", () => {
    return <Notification type={NotificationType.danger}>test</Notification>;
  });
