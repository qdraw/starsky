import { ImageObject, PositionObject } from "./pan-and-zoom-image";

export class OnWheelMouseAction {
  private image: ImageObject;
  private setPosition: React.Dispatch<React.SetStateAction<PositionObject>>;
  private position: PositionObject;
  private containerRef: React.RefObject<HTMLDivElement>;
  private onWheelCallback: (z: number) => void;

  constructor(
    image: ImageObject,
    setPosition: React.Dispatch<React.SetStateAction<PositionObject>>,
    position: PositionObject,
    containerRef: React.RefObject<HTMLDivElement>,
    onWheelCallback: (z: number) => void
  ) {
    this.image = image;
    this.setPosition = setPosition;
    this.position = position;
    this.containerRef = containerRef;
    this.onWheelCallback = onWheelCallback;

    // bind this to object
    this.onWheel = this.onWheel.bind(this);
  }

  public onWheel(e: React.WheelEvent<HTMLDivElement>) {
    if (!e.deltaY) return;
    this.zoom(e.deltaY, e.clientX);
  }

  /**
   * Set position for zoom
   * @param eventDeltaY : -1 is zoom in, +1 zoom out
   * @param eventclientX pixels calc form left of screen
   * @returns void
   */
  public zoom(eventDeltaY: number, eventclientX: number = 0) {
    const sign = Math.sign(eventDeltaY) / 10;
    const scale = 1 - sign;

    if (!this.containerRef.current) {
      return;
    }
    const rect = this.containerRef.current.getBoundingClientRect();
    // default is to align to the center
    if (eventDeltaY <= 0) eventDeltaY = rect.height / 2;
    if (eventclientX <= 0) eventclientX = rect.width / 2;
    if (!rect.x) rect.x = 0;
    if (!rect.y) rect.y = 0;

    const z = this.position.z * scale;
    // DEBUG
    console.log({
      ...this.position,
      x:
        this.position.x * scale -
        (rect.width / 2 - eventclientX + rect.x) * sign,
      y:
        this.position.y * scale -
        ((this.image.height * rect.width) / this.image.width / 2 -
          eventclientX +
          rect.y) *
          sign,
      z
    });

    this.setPosition({
      ...this.position,
      x:
        this.position.x * scale -
        (rect.width / 2 - eventclientX + rect.x) * sign,
      y:
        this.position.y * scale -
        ((this.image.height * rect.width) / this.image.width / 2 -
          eventclientX +
          rect.y) *
          sign,
      z
    });

    this.onWheelCallback(z);
  }
}
