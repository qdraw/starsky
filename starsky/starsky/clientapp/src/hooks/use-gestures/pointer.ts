export class Pointer {
  public x: number = 0;
  public y: number = 0;

  /**
   * Point element
   * @param {{clientX:number, clientY: number}} touch event touch object
   */
  constructor(touch: any) {
    this.x = touch.clientX;
    this.y = touch.clientY;
  }
}
