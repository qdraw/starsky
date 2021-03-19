import { ImageObject } from "./pan-and-zoom-image";

export class OnLoadMouseAction {
  setImage: React.Dispatch<React.SetStateAction<ImageObject>>;
  setError?: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;

  constructor(
    setImage: React.Dispatch<React.SetStateAction<ImageObject>>,
    setError?: React.Dispatch<React.SetStateAction<boolean>>,
    setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>
  ) {
    this.setImage = setImage;
    this.setError = setError;
    this.setIsLoading = setIsLoading;
  }

  public onLoad = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    const target = e.target as any;
    this.setImage({
      width: target.naturalWidth,
      height: target.naturalHeight
    });

    if (!this.setError || !this.setIsLoading) {
      return;
    }

    this.setError(false);
    this.setIsLoading(false);
  };
}
