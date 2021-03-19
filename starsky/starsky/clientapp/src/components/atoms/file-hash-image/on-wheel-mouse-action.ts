import { ImageObject, PositionObject } from "./pan-and-zoom-image";

export class OnWheelMouseAction {
  image: ImageObject;
  setPosition: React.Dispatch<React.SetStateAction<PositionObject>>;
  position: PositionObject;
  containerRef: React.RefObject<HTMLDivElement>;
  onWheelCallback: () => void;

  constructor(
    image: ImageObject,
    setPosition: React.Dispatch<React.SetStateAction<PositionObject>>,
    position: PositionObject,
    containerRef: React.RefObject<HTMLDivElement>,
    onWheelCallback: () => void
  ) {
    this.image = image;
    this.setPosition = setPosition;
    this.position = position;
    this.containerRef = containerRef;
    this.onWheelCallback = onWheelCallback;
  }

  public onWheel = (e: React.WheelEvent<HTMLDivElement>) => {
    if (e.deltaY) {
      const sign = Math.sign(e.deltaY) / 10;
      const scale = 1 - sign;

      if (!this.containerRef.current) {
        return;
      }
      const rect = this.containerRef.current.getBoundingClientRect();

      this.setPosition({
        ...this.position,
        x:
          this.position.x * scale -
          (rect.width / 2 - e.clientX + rect.x) * sign,
        y:
          this.position.y * scale -
          ((this.image.height * rect.width) / this.image.width / 2 -
            e.clientY +
            rect.y) *
            sign,
        z: this.position.z * scale
      });

      this.onWheelCallback();
    }
  };
}
