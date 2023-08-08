import Portal, { PortalId } from "../portal/portal";

type NotificationPropTypes = {
  children?: React.ReactNode;
  type?: NotificationType;
  callback?(): void;
};

export enum NotificationType {
  default = "default",
  danger = "danger"
}

const Notification: React.FunctionComponent<NotificationPropTypes> = ({
  children,
  type,
  callback
}) => {
  const close = () => {
    // remove the entire portal
    const portal = document.getElementById(PortalId);
    if (portal) {
      portal.remove();
    }
    if (callback) {
      callback();
    }
  };

  return (
    <Portal>
      <div className={`notification notification--${type}`}>
        <div className="icon icon--error" />
        <div data-test="notification-content" className="content">
          {children}
        </div>
        <button
          data-test="notification-close"
          className="icon icon--close"
          onClick={close}
        />
      </div>
    </Portal>
  );
};

export default Notification;
