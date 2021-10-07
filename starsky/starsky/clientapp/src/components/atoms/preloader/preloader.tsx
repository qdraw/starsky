import React from "react";

export interface IPreloaderProps {
  isOverlay: boolean;
  isWhite?: boolean;
  isTransition?: boolean;
  parent?: string;
}

const Preloader: React.FunctionComponent<IPreloaderProps> = (props) => {
  const isWhiteClassName = props.isWhite ? "preloader--white" : null;
  return (
    <>
      {props.isOverlay ? (
        <div
          data-test="preloader"
          className={
            props.isTransition === false
              ? "preloader preloader--overlay-no-transition"
              : "preloader preloader--overlay"
          }
        >
          <div className="preloader preloader--icon"></div>
        </div>
      ) : (
        <div className={`preloader preloader--icon ${isWhiteClassName}`}></div>
      )}
    </>
  );
};

export default Preloader;
