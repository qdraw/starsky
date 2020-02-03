import React from 'react';
import Portal, { PortalId } from './portal';

type NotificationPropTypes = {
  children?: React.ReactNode;
  className?: string;
}

const Notification: React.FunctionComponent<NotificationPropTypes> = ({ children, className }) => {
  return (
    <Portal>
      <div className={"notification " + className}>
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
