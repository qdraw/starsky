
class AspectRatio {

  /**
   * 
   * In mathematics, the greatest common divisor (gcd) of two or more integers,
   * which are not all zero, is the largest positive integer that divides each of the integers.
   * For example, the gcd of 8 and 12 is 4.
   * @param u first input value
   * @param v second input value
   * @see https://stackoverflow.com/a/50260127
   */
  public gcd(u: number, v: number): number {
    if (u === v) return u;
    if (u === 0) return v;
    if (v === 0) return u;

    if (~u & 1)
      if (v & 1)
        return this.gcd(u >> 1, v);
      else
        return this.gcd(u >> 1, v >> 1) << 1;

    if (~v & 1) return this.gcd(u, v >> 1);

    if (u > v) return this.gcd((u - v) >> 1, v);

    return this.gcd((v - u) >> 1, u);
  }

  /**
   * Get the aspect ratio of an image
   * @param width in pixels
   * @param height and the height
   */
  public ratio(width: number, height: number): string {
    var r = this.gcd(width, height);
    return width / r + ":" + height / r
  }

}

export default AspectRatio;