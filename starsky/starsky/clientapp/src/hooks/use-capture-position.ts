// capturePosition

export interface ICaptionPosition {
  freeze: () => void;
  unfreeze: () => void;
}

const capturePosition = () => {
  let cachedPosition = window.pageYOffset;
  return {
    freeze: () => {
      (document.body as any).style.position =
        `fixed; top: ${cachedPosition * -1}px; width: 100%;`;
    },
    unfreeze: () => {
      document.body.removeAttribute("style");
      window.scrollTo({
        top: cachedPosition
      });
    }
  } as ICaptionPosition;
};
export default capturePosition;