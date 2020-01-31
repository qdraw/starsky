import { useEffect, useState } from 'react';
import ReactDOM from 'react-dom';

type PortalPropTypes = {
  children?: React.ReactNode;
}

const Portal: React.FunctionComponent<PortalPropTypes> = ({ children }) => {
  const [modalContainer] = useState(document.createElement('div'));
  useEffect(() => {
    // Find the root element in your DOM
    let modalRoot = document.getElementById('portal-root') as HTMLElement;
    // If there is no root then create one
    if (!modalRoot) {
      const tempEl = document.createElement('div');
      tempEl.id = 'portal-root';
      document.body.append(tempEl);
      modalRoot = tempEl;
    }
    // Append modal container to root
    modalRoot.appendChild(modalContainer);
    return function cleanup() {
      // On cleanup remove the modal container
      modalRoot.remove();
    };
  }, [modalContainer]);
  // ^^- The empty array with modalContainer 
  // tells react to apply the effect on mount / unmount

  return ReactDOM.createPortal(children, modalContainer);
};

export default Portal
