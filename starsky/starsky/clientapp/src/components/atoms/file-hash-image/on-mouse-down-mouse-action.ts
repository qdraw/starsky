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

  public onMouseDown = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
    e.preventDefault();
    this.setPanning(true);
    this.setPosition({
      ...this.position,
      oldX: e.clientX,
      oldY: e.clientY
    });
  };
}
