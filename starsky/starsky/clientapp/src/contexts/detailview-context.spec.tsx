import { IDetailView, IRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import { detailviewReducer } from './detailview-context';

describe("DetailViewContext", () => {
  it("update - check if item is update (append false)", () => {
    const state = {
      breadcrumb: [],
      fileIndexItem: newIFileIndexItem(),
      relativeObjects: {} as IRelativeObjects,
      subPath: "/",
      pageType: PageType.DetailView,
      colorClassActiveList: [],
      isReadOnly: false,
    } as IDetailView;
    var action = { type: 'update', tags: 'tags', colorclass: 1, description: 'description', title: 'title', status: IExifStatus.Ok } as any;

    var result = detailviewReducer(state, action);

    expect(result.fileIndexItem.tags).toBe('tags');
    expect(result.fileIndexItem.colorClass).toBe(1);
    expect(result.fileIndexItem.description).toBe('description');
    expect(result.fileIndexItem.title).toBe('title');
  });

  it("append - check if item is update", () => {
    const state = {
      breadcrumb: [],
      fileIndexItem: newIFileIndexItem(),
      relativeObjects: {} as IRelativeObjects,
      subPath: "/",
      pageType: PageType.DetailView,
      colorClassActiveList: [],
      isReadOnly: false,
    } as IDetailView;
    var action = { type: 'append', tags: 'tags' } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe(',tags');
  });

  it("remove - check if item is update", () => {
    const state = {
      breadcrumb: [],
      fileIndexItem: newIFileIndexItem(),
      relativeObjects: {} as IRelativeObjects,
      subPath: "/",
      pageType: PageType.DetailView,
      colorClassActiveList: [],
      isReadOnly: false,
    } as IDetailView;
    state.fileIndexItem.tags = "!delete!"

    const action = {type: 'remove', tags: '!delete!'} as any;

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe('');
  });

});
