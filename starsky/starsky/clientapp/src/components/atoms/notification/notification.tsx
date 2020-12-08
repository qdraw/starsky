import React from "react";
import Portal, { PortalId } from "../portal/portal";

type NotificationPropTypes = {
  children?: React.ReactNode;
  type?: NotificationType;
  callback?(): void;
};

export enum NotificationType {
  default = "default" as any,
  danger = "danger" as any
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
        <div className="content">{children}</div>
        <button className="icon icon--close" onClick={close} />
      </div>
    </Portal>
  );
};

export default Notification;
