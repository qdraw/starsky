import { PositionObject } from "./pan-and-zoom-image";

export class OnMoveMouseTouchAction {
  private setPosition: React.Dispatch<React.SetStateAction<PositionObject>>;
  private position: PositionObject;
  private isPanning: boolean;

  constructor(
    isPanning: boolean,
    setPosition: React.Dispatch<React.SetStateAction<PositionObject>>,
    position: PositionObject
  ) {
    this.isPanning = isPanning;
    this.setPosition = setPosition;
    this.position = position;

    // bind this to object
    this.move = this.move.bind(this);
  }

  public touchMove = (e: TouchEvent) => {
    if (!e.touches) return;
    const clientX = e.touches[0].clientX;
    const clientY = e.touches[0].clientY;
    this.move(clientX, clientY);
  };

  public mousemove = (event: MouseEvent) => {
    this.move(event.clientX, event.clientY);
  };

  public move(clientX: number, clientY: number) {
    if (this.isPanning && this.position.y !== 0) {
      this.setPosition({
        ...this.position,
        x: this.position.x + clientX - this.position.oldX,
        y: this.position.y + clientY - this.position.oldY,
        oldX: clientX,
        oldY: clientY
      });
    }
  }
}
