import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { UpdateRelativeObject } from "../../../shared/update-relative-object";
import { UrlQuery } from "../../../shared/url-query";

export class PrevNext {
  relativeObjects: IRelativeObjects;
  state: IDetailView;
  isSearchQuery: boolean;
  history: IUseLocation;
  setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;

  constructor(
    relativeObjects: IRelativeObjects,
    state: IDetailView,
    isSearchQuery: boolean,
    history: IUseLocation,
    setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>,
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>
  ) {
    this.relativeObjects = relativeObjects;
    this.state = state;
    this.isSearchQuery = isSearchQuery;
    this.history = history;
    this.setRelativeObjects = setRelativeObjects;
    this.setIsLoading = setIsLoading;
  }

  /**
   * navigation function to go to next photo
   */
  next() {
    if (!this.relativeObjects) return;
    if (!this.relativeObjects.nextFilePath) return;
    if (this.relativeObjects.nextFilePath === this.state.subPath) {
      // when changing next very fast it might skip a check
      new UpdateRelativeObject()
        .Update(
          this.state,
          this.isSearchQuery,
          this.history.location.search,
          this.setRelativeObjects
        )
        .then((data) => {
          this.navigateNext(data);
        })
        .catch(() => {
          // do nothing on catch error
        });
      return;
    }
    this.navigateNext(this.relativeObjects);
  }

  /**
   * Navigate to Next
   * @param relative object to move from
   */
  navigateNext(relative: IRelativeObjects) {
    const nextPath = new UrlQuery().updateFilePathHash(
      this.history.location.search,
      relative.nextFilePath,
      false
    );
    // Prevent keeps loading forever
    if (relative.nextHash !== this.state.fileIndexItem.fileHash) {
      this.setIsLoading(true);
    }

    this.history.navigate(nextPath, { replace: true }).then(() => {
      this.setIsLoading(false);
    });
  }

  /**
   * navigation function to go to prev photo
   */
  prev() {
    if (!this.relativeObjects) return;
    if (!this.relativeObjects.prevFilePath) return;
    if (this.relativeObjects.prevFilePath === this.state.subPath) {
      // when changing prev very fast it might skip a check
      new UpdateRelativeObject()
        .Update(
          this.state,
          this.isSearchQuery,
          this.history.location.search,
          this.setRelativeObjects
        )
        .then((data) => {
          this.navigatePrev(data);
        });
      return;
    }
    this.navigatePrev(this.relativeObjects);
  }

  /**
   * Navigate to prev
   * @param relative object to move from
   */
  navigatePrev(relative: IRelativeObjects) {
    const prevPath = new UrlQuery().updateFilePathHash(
      this.history.location.search,
      this.relativeObjects.prevFilePath,
      false
    );

    // Prevent keeps loading forever
    if (relative.prevHash !== this.state.fileIndexItem.fileHash) {
      this.setIsLoading(true);
    }

    this.history.navigate(prevPath, { replace: true }).then(() => {
      // when the re-render happens un-expected
      // window.location.search === history.location.search
      this.setIsLoading(false);
    });
  }
}
