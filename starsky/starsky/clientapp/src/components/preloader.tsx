import React, { memo } from 'react';

export interface IPreloaderProps {
  isOverlay: boolean;
  isDetailMenu: boolean;
  parent?: string;
}

const Preloader: React.FunctionComponent<IPreloaderProps> = memo((props) => {

  return (
    <>
      {/* {props.isDetailMenu ? <MenuDetailView parent={props.parent}></MenuDetailView> : <Menu></Menu>} */}
      {
        props.isOverlay ? <div className={"preloader preloader--overlay"}>
          <div className="preloader preloader--icon">
          </div>
        </div> : <div className="preloader preloader--icon">
          </div>
      }

    </>
  );
});

export default Preloader