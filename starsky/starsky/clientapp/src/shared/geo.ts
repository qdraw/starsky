export class Geo {
  /**
   * Get the distance of two cordinates (over the air)
   * @param point1 example: [52.636206, 4.657292]
   * @param point2 so lat log in a array
   */
  public Distance(point1: number[], point2: number[]) {
    if (!point1 || point1.length !== 2) throw Error("point 1 has wrong input");
    if (!point2 || point2.length !== 2) throw Error("point 2 has wrong input");

    const lat = [point1[0], point2[0]];
    const lng = [point1[1], point2[1]];

    const R = 6378137;
    const dLat = ((lat[1] - lat[0]) * Math.PI) / 180;
    const dLng = ((lng[1] - lng[0]) * Math.PI) / 180;
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos((lat[0] * Math.PI) / 180) *
        Math.cos((lat[1] * Math.PI) / 180) *
        Math.sin(dLng / 2) *
        Math.sin(dLng / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const d = R * c;
    return Math.round(d);
  }

  public Validate(lat: number, long: number): boolean {
    const latitudeRegex = /^[+-]?(([1-8]?[0-9])(\.[0-9]{1,6})?|90(\.0{1,6})?)$/;
    const longitudeRegex =
      /^[+-]?((([1-9]?[0-9]|1[0-7][0-9])(\.[0-9]{1,6})?)|180(\.0{1,6})?)$/;

    const latRound = Math.round(lat * 1000000) / 1000000;
    const longRound = Math.round(long * 1000000) / 1000000;

    return (
      latitudeRegex.test(latRound.toString()) &&
      longitudeRegex.test(longRound.toString())
    );
  }
}
