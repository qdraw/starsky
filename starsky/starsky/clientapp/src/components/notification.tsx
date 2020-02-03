import React from 'react';
import Portal, { PortalId } from './portal';

type NotificationPropTypes = {
  children?: React.ReactNode;
  type?: NotificationType;
}

export enum NotificationType {
  default = "default" as any,
  danger = "danger" as any,
}

const Notification: React.FunctionComponent<NotificationPropTypes> = ({ children, type }) => {

  const close = () => {
    const portal = document.getElementById(PortalId);
    if (!portal) return;
    portal.remove();
  };

  return (
    <Portal>
      <div className={"notification notification--" + type}>
        <div className="icon icon--error" />
        <div className="content">
          {children}
        </div>
        <button className="icon icon--close" onClick={close} />
      </div>
    </Portal>
  );
};

export default Notification
