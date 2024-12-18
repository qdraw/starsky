// capturePosition

export interface ICaptionPosition {
  freeze: () => void;
  unfreeze: () => void;
}

const capturePosition = () => {
  const topCachedPosition = window.scrollY;
  return {
    freeze: () => {
      document.body.style.position = "fixed";
      document.body.style.top = `${topCachedPosition * -1}px`;
      document.body.style.width = "100%";
    },
    unfreeze: () => {
      document.body.style.position = "initial";
      document.body.style.top = "initial";
      document.body.style.width = "initial";
      window.scrollTo(0, topCachedPosition);
    }
  } as ICaptionPosition;
};
export default capturePosition;
