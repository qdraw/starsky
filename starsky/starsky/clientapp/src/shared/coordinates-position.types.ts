/* get from lib-dom */
export interface Coordinates {
	readonly accuracy: number;
	readonly altitude: number | null;
	readonly altitudeAccuracy: number | null;
	readonly heading: number | null;
	readonly latitude: number;
	readonly longitude: number;
	readonly speed: number | null;
}

export interface Position {
	readonly coords: Coordinates;
	readonly timestamp: number;
}
