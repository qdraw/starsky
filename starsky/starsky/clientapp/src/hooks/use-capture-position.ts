// capturePosition

export interface ICaptionPosition {
  freeze: () => void;
  unfreeze: () => void;
}

const capturePosition = () => {
  let topCachedPosition = window.pageYOffset;
  return {
    freeze: () => {
      (document.body as any).style.position = "fixed";
      (document.body as any).style.top = `${topCachedPosition * -1}px`;
      (document.body as any).style.width = "100%";
    },
    unfreeze: () => {
      (document.body as any).style.position = "initial";
      (document.body as any).style.top = "initial";
      (document.body as any).style.width = "initial";
      window.scrollTo(0, topCachedPosition);
    }
  } as ICaptionPosition;
};
export default capturePosition;
