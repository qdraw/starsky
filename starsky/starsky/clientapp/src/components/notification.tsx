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
  return (
    <Portal>
      <div className={"notification notification--" + type}>
        <div className="icon icon--error"></div>
        <div className="content">
          {children}
        </div>
        <button className="icon icon--close" onClick={() => { document.getElementById(PortalId)?.remove() }}></button>
      </div>
    </Portal>
  );
};

export default Notification
