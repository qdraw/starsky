import React, { memo } from 'react';

export interface IPreloaderProps {
  isOverlay: boolean;
  isDetailMenu?: boolean;
  isTransition?: boolean;
  parent?: string;
}

const Preloader: React.FunctionComponent<IPreloaderProps> = memo((props) => {
  return (
    <>
      {
        props.isOverlay ? <div className={props.isTransition === false ? "preloader preloader--overlay-no-transition" : "preloader preloader--overlay"}>
          <div className="preloader preloader--icon">
          </div>
        </div> : <div className="preloader preloader--icon">
          </div>
      }
    </>
  );
});

export default Preloader
