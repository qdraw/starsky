import React from "react";

export interface IPreloaderProps {
  isOverlay: boolean;
  isWhite?: boolean;
  isTransition?: boolean;
  parent?: string;
}

const Preloader: React.FunctionComponent<IPreloaderProps> = (props) => {
  return (
    <>
      {props.isOverlay ? (
        <div
          className={
            props.isTransition === false
              ? "preloader preloader--overlay-no-transition"
              : "preloader preloader--overlay"
          }
        >
          <div className="preloader preloader--icon"></div>
        </div>
      ) : (
        <div
          className={`preloader preloader--icon ${
            props.isWhite ? "preloader--white" : null
          }`}
        ></div>
      )}
    </>
  );
};

export default Preloader;
