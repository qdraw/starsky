import React, { useEffect, useRef, useState } from "react";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import { OnLoadMouseAction } from "./on-load-mouse-action";
import { OnMouseDownMouseAction } from "./on-mouse-down-mouse-action";
import { OnMoveMouseTouchAction } from "./on-move-mouse-touch-action";
import { OnWheelMouseAction } from "./on-wheel-mouse-action";

export interface IPanAndZoomImage {
  src: string;
  setError?: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;
  translateRotation: Orientation;
  id?: string; // to known when an image is changed
  alt?: string;

  onErrorCallback?(): void;

  onWheelCallback(z: number): void;

  onResetCallback(): void;
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
const PanAndZoomImage = ({ src, id, alt, ...props }: IPanAndZoomImage) => {
  const [panning, setPanning] = useState(false);
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
    const isLoaded = imageReference?.complete && imageReference?.naturalHeight !== 0;
    props.setIsLoading(isLoaded === false);

    // use Memo gives issues elsewhere
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [id]);

  const containerRef = useRef<HTMLButtonElement>(null);

  // only needed to re-render when the component is mounted

  useEffect(() => {
    const mouseup = () => {
      setPanning(false);
    };
    // for performance reasons the classes is kept in a function
    const mouseMove = (event: MouseEvent) => {
      new OnMoveMouseTouchAction(panning, setPosition, position).mousemove(event);
    };
    const touchMove = (event: TouchEvent) => {
      new OnMoveMouseTouchAction(panning, setPosition, position).touchMove(event);
    };

    window.addEventListener("mouseup", mouseup);
    window.addEventListener("mousemove", mouseMove);
    window.addEventListener("touchmove", touchMove);

    return () => {
      window.removeEventListener("mouseup", mouseup);
      window.removeEventListener("mousemove", mouseMove);
      window.removeEventListener("touchmove", touchMove);
    };
  });

  function zoomIn() {
    new OnWheelMouseAction(image, setPosition, position, containerRef, props.onWheelCallback).zoom(
      -1
    );
  }

  function zoomOut() {
    new OnWheelMouseAction(image, setPosition, position, containerRef, props.onWheelCallback).zoom(
      1
    );
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

  let className = "pan-zoom-image-container";
  if (position.z !== 1) {
    className = !panning ? "pan-zoom-image-container grab" : "pan-zoom-image-container is-panning";
  }

  return (
    <>
      <button
        className={className}
        data-test="pan-zoom-image"
        ref={containerRef}
        onMouseDown={new OnMouseDownMouseAction(setPanning, position, setPosition).onMouseDown}
        onTouchStart={(touchEvent) =>
          new OnMouseDownMouseAction(setPanning, position, setPosition).onTouchStart(
            touchEvent as unknown as TouchEvent
          )
        }
        onWheel={
          new OnWheelMouseAction(image, setPosition, position, containerRef, props.onWheelCallback)
            .onWheel
        }
      >
        <div
          data-test="pan-zoom-image-container"
          style={{
            transform: `translate(${position.x}px, ${position.y}px) scale(${position.z})`
          }}
        >
          {/* NOSONAR(S6847) */}
          <img
            className={`pan-zoom-image--image image--default ${props.translateRotation}`}
            alt={alt}
            src={src}
            onLoad={new OnLoadMouseAction(setImage, props.setError, props.setIsLoading).onLoad}
            onError={() => {
              if (!props.setError || !props.setIsLoading) {
                return;
              }
              props.setError(true);
              props.setIsLoading(false);
              if (props.onErrorCallback) props.onErrorCallback();
            }}
          />
        </div>
      </button>
      <div className="gpx-controls">
        <div className="gpx-controls--button">
          <button
            data-test="zoom_in"
            title={"Zoom in"}
            className="icon icon--zoom_in"
            onClick={() => zoomIn()}
          >
            Zoom in
          </button>
        </div>
        <div className="gpx-controls--button">
          <button
            title={"Zoom out"}
            data-test="zoom_out"
            className="icon icon--zoom_out"
            onClick={() => zoomOut()}
          >
            Zoom out
          </button>
        </div>
        <div className="gpx-controls--button">
          <button
            disabled={position.z === 1}
            data-test="zoom_reset"
            title={"Reset zoom"}
            className={"icon icon--zoom_reset"}
            onClick={() => reset()}
          >
            Zoom in
          </button>
        </div>
        <div className="gpx-controls--button spacer"></div>
      </div>
    </>
  );
};

export default PanAndZoomImage;
