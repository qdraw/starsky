import React, { useEffect, useRef, useState } from "react";
import { Orientation } from "../../../interfaces/IFileIndexItem";

export interface IPanAndZoomImage {
  src: string;
  setError?: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;
  translateRotation: Orientation;
  onWheelCallback(): void;
  id?: string; // to known when a image is changed
}

/**
 * @see: jkettmann.com/jr-to-sr-refactoring-react-pan-and-zoom-image-component
 * @param param0:  IPanAndZoomImage
 */
const PanAndZoomImage = ({ src, id, ...props }: IPanAndZoomImage) => {
  const [isPanning, setPanning] = useState(false);
  const [image, setImage] = useState({ width: 0, height: 0 });

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
    // use Memo gives issues elsewhere
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  useEffect(() => {
    if (!props.setIsLoading) return;
    props.setIsLoading(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [src]);

  const containerRef = useRef<HTMLDivElement>(null);

  const onLoad = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    console.log("--onload");

    const target = e.target as any;
    setImage({
      width: target.naturalWidth,
      height: target.naturalHeight
    });

    if (!props.setError || !props.setIsLoading) {
      return;
    }

    props.setError(false);
    props.setIsLoading(false);
  };

  const onMouseDown = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
    e.preventDefault();
    setPanning(true);
    setPosition({
      ...position,
      oldX: e.clientX,
      oldY: e.clientY
    });
  };

  const onWheel = (e: React.WheelEvent<HTMLDivElement>) => {
    if (e.deltaY) {
      const sign = Math.sign(e.deltaY) / 10;
      const scale = 1 - sign;

      if (!containerRef.current) {
        return;
      }
      const rect = containerRef.current.getBoundingClientRect();

      setPosition({
        ...position,
        x: position.x * scale - (rect.width / 2 - e.clientX + rect.x) * sign,
        y:
          position.y * scale -
          ((image.height * rect.width) / image.width / 2 - e.clientY + rect.y) *
            sign,
        z: position.z * scale
      });

      props.onWheelCallback();
    }
  };

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

  return (
    <div
      className={
        !isPanning
          ? "pan-zoom-image-container grab"
          : "pan-zoom-image-container is-panning"
      }
      ref={containerRef}
      onMouseDown={onMouseDown}
      onWheel={onWheel}
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
          onLoad={onLoad}
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
  );
};

export default PanAndZoomImage;
