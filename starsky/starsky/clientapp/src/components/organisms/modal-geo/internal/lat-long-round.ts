export function LatLongRound(latitudeLong: number | undefined) {
  return latitudeLong ? Math.round(latitudeLong * 1000000) / 1000000 : 0;
}
