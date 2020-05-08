import { storiesOf } from "@storybook/react";
import React from "react";
import Notification, { NotificationType } from './notification';

storiesOf("components/atoms/notification", module)
  .add("default", () => {
    return <Notification type={NotificationType.default}>test</Notification>
  })
  .add("danger", () => {
    return <Notification type={NotificationType.danger}>test</Notification>
  })