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
    console.log("-touchStart", clientX, clientY);

    e.preventDefault();
    this.down(clientX, clientY);
  };

  public onMouseDown = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
    e.preventDefault();
    this.down(e.clientX, e.clientY);
  };

  public down = (clientX: number, clientY: number) => {
    this.setPanning(true);
    console.log(this.position);

    this.setPosition({
      ...this.position,
      oldX: clientX,
      oldY: clientY
    });
  };
}
