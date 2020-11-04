import React from 'react';
import Portal, { PortalId } from '../portal/portal';

type NotificationPropTypes = {
  children?: React.ReactNode;
  type?: NotificationType;
  callback?(): void;
}

export enum NotificationType {
  default = "default" as any,
  danger = "danger" as any,
}

const Notification: React.FunctionComponent<NotificationPropTypes> = ({ children, type, callback }) => {

  const close = () => {
    if (!notificationRef.current) return;
    notificationRef.current.remove();
    // clean only when the last one is closed
    const portal = document.getElementById(PortalId);
    if (portal && portal.querySelectorAll('.notification').length === 0) {
      portal.remove();
    }
    if (callback) callback();
  };

  const notificationRef = React.useRef<HTMLDivElement>(null);

  return (
    <Portal>
      <div className={`notification notification--${type}`} ref={notificationRef}>
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
