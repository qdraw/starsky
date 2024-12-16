import { Pointer } from "./pointer";

export interface ICurrentTouches {
  preventDefault: () => void;
  stopPropagation?: () => void;
  pointers?: Pointer[];
  delta: number;
  scale?: number;
  distance: number;
  angleDeg: number;
  deltaX?: number;
  deltaY?: number;
  x?: number;
  y?: number;
}
