import { PositionObject } from "./pan-and-zoom-image";

export class OnMouseDownMouseAction {
  setPosition: React.Dispatch<React.SetStateAction<PositionObject>>;
  setPanning: React.Dispatch<React.SetStateAction<boolean>>;
  position: PositionObject;

  constructor(
    setPanning: React.Dispatch<React.SetStateAction<boolean>>,
    position: PositionObject,
    setPosition: React.Dispatch<React.SetStateAction<PositionObject>>
  ) {
    this.setPanning = setPanning;
    this.position = position;
    this.setPosition = setPosition;
  }

  public onTouchStart = (e: TouchEvent) => {
    const clientX = e.touches[0].clientX;
    const clientY = e.touches[0].clientY;
    e.preventDefault();
    this.move(clientX, clientY);
  };

  public onMouseDown = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
    e.preventDefault();
    this.move(e.clientX, e.clientY);
  };

  public move = (clientX: number, clientY: number) => {
    this.setPanning(true);
    this.setPosition({
      ...this.position,
      oldX: clientX,
      oldY: clientY
    });
  };
}
