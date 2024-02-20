import Notification, { NotificationType } from "./notification";

export default {
  title: "components/atoms/notification"
};

export const Default = () => {
  return (
    <Notification type={NotificationType.default}>
      &quot;There are critical errors in the following components:&quot;
    </Notification>
  );
};

Default.storyName = "default";

export const Danger = () => {
  return <Notification type={NotificationType.danger}>test</Notification>;
};

Danger.storyName = "danger";
