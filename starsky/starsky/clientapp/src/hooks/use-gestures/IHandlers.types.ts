export interface IHandlers {
  onPanStart?: () => void;
  onPanMove?: (ev: any) => void;
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
