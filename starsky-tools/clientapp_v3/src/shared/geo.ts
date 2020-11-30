
export class Geo {

  /**
   * Get the distance of two cordinates (over the air)
   * @param point1 example: [52.636206, 4.657292]
   * @param point2 so lat log in a array
   */
  public Distance(point1: number[], point2: number[]) {

    if (!point1 || point1.length !== 2) throw Error("point 1 has wrong input");
    if (!point2 || point2.length !== 2) throw Error("point 2 has wrong input");

    var lat = [point1[0], point2[0]];
    var lng = [point1[1], point2[1]];

    var R = 6378137;
    var dLat = (lat[1] - lat[0]) * Math.PI / 180;
    var dLng = (lng[1] - lng[0]) * Math.PI / 180;
    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(lat[0] * Math.PI / 180) * Math.cos(lat[1] * Math.PI / 180) *
      Math.sin(dLng / 2) * Math.sin(dLng / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c;
    return Math.round(d);
  }
}