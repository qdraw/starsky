import React from 'react';
import Portal from './portal';

type NotificationPropTypes = {
  children?: React.ReactNode;
  className?: string;
}

const Notification: React.FunctionComponent<NotificationPropTypes> = ({ children, className }) => {
  return (
    <Portal>
      <div className={"notification " + className}>
        <div className="icon"></div>
        <div className="content">
          {children}
        </div>
      </div>
    </Portal>
  );
};

export default Notification
