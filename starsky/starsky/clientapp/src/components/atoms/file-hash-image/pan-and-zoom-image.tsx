import React, { useEffect, useRef, useState } from "react";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import { OnLoadMouseAction } from "./on-load-mouse-action";
import { OnMouseDownMouseAction } from "./on-mouse-down-mouse-action";
import { OnWheelMouseAction } from "./on-wheel-mouse-action";

export interface IPanAndZoomImage {
  src: string;
  setError?: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;
  translateRotation: Orientation;
  onWheelCallback(): void;
  onResetCallback(): void;
  id?: string; // to known when a image is changed
}

export type ImageObject = { width: number; height: number };
export type PositionObject = {
  oldX: number;
  oldY: number;
  x: number;
  y: number;
  z: number;
};

/**
 * @see: jkettmann.com/jr-to-sr-refactoring-react-pan-and-zoom-image-component
 * @param param0:  IPanAndZoomImage
 */
const PanAndZoomImage = ({ src, id, ...props }: IPanAndZoomImage) => {
  const [isPanning, setPanning] = useState(false);
  const [image, setImage] = useState({ width: 0, height: 0 } as ImageObject);

  const defaultPosition = {
    oldX: 0,
    oldY: 0,
    x: 0,
    y: 0,
    z: 1
  };
  const [position, setPosition] = useState(defaultPosition);

  useEffect(() => {
    setPosition(defaultPosition);
    if (!props.setIsLoading) return;

    const imageReference = containerRef.current?.querySelector("img");
    const isLoaded =
      imageReference &&
      imageReference.complete &&
      imageReference.naturalHeight !== 0;
    props.setIsLoading(isLoaded === false);

    // use Memo gives issues elsewhere
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const mouseup = () => {
      setPanning(false);
    };

    const mousemove = (event: MouseEvent) => {
      if (isPanning) {
        setPosition({
          ...position,
          x: position.x + event.clientX - position.oldX,
          y: position.y + event.clientY - position.oldY,
          oldX: event.clientX,
          oldY: event.clientY
        });
      }
    };

    window.addEventListener("mouseup", mouseup);
    window.addEventListener("mousemove", mousemove);

    return () => {
      window.removeEventListener("mouseup", mouseup);
      window.removeEventListener("mousemove", mousemove);
    };
  });

  function zoomIn() {
    new OnWheelMouseAction(
      image,
      setPosition,
      position,
      containerRef,
      props.onWheelCallback
    ).zoom(-1);
  }

  function zoomOut() {
    new OnWheelMouseAction(
      image,
      setPosition,
      position,
      containerRef,
      props.onWheelCallback
    ).zoom(1);
  }

  function reset() {
    setPosition(defaultPosition);
    props.onResetCallback();
  }

  // zoom in
  useHotKeys({ key: "=", ctrlKeyOrMetaKey: true }, zoomIn, []);
  // zoom out
  useHotKeys({ key: "-", ctrlKeyOrMetaKey: true }, zoomOut, []);
  // and reset
  useHotKeys({ key: "0", ctrlKeyOrMetaKey: true }, reset, []);

  return (
    <>
      <div
        className={
          !isPanning
            ? "pan-zoom-image-container grab"
            : "pan-zoom-image-container is-panning"
        }
        ref={containerRef}
        onMouseDown={
          new OnMouseDownMouseAction(setPanning, position, setPosition)
            .onMouseDown
        }
        onTouchStart={(e) =>
          new OnMouseDownMouseAction(
            setPanning,
            position,
            setPosition
          ).onTouchStart(e as any)
        }
        onWheel={
          new OnWheelMouseAction(
            image,
            setPosition,
            position,
            containerRef,
            props.onWheelCallback
          ).onWheel
        }
      >
        <div
          style={{
            transform: `translate(${position.x}px, ${position.y}px) scale(${position.z})`
          }}
        >
          <img
            className={`pan-zoom-image--image image--default ${props.translateRotation}`}
            alt="floorplan"
            src={src}
            onLoad={
              new OnLoadMouseAction(
                setImage,
                props.setError,
                props.setIsLoading
              ).onLoad
            }
            onError={() => {
              if (!props.setError || !props.setIsLoading) {
                return;
              }
              props.setError(true);
              props.setIsLoading(false);
            }}
          />
        </div>
      </div>
      <div className="gpx-controls">
        <button
          data-test="zoom_in"
          title={"Zoom in"}
          className="icon icon--zoom_in"
          onClick={() => zoomIn()}
        >
          Zoom in
        </button>
        <button
          title={"Zoom out"}
          data-test="zoom_out"
          className="icon icon--zoom_out"
          onClick={() => zoomOut()}
        >
          Zoom out
        </button>
      </div>
    </>
  );
};

export default PanAndZoomImage;
