import { ICurrentTouches } from "./ICurrentTouches.types";

export interface IHandlers {
  onPanStart?: () => void;
  onPanMove?: (ev: TouchEvent) => void;
  onSwipeLeft?: () => void;
  onSwipeRight?: () => void;
  onSwipeUp?: () => void;
  onSwipeDown?: () => void;
  onPanEnd?: () => void;
  onSwipeLeftEnd?: () => void;
  onSwipeRightEnd?: () => void;
  onSwipeUpEnd?: () => void;
  onSwipeDownEnd?: () => void;
  onPinchStart?: () => void;
  onPinchChanged?: () => void;
  onPinchEnd?: () => void;
}

export interface IHandlersMapper {
  [key: string]: (event?: ICurrentTouches) => void;
}
