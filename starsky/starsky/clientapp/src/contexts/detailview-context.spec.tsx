import { IDetailView, IRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import { detailviewReducer } from './detailview-context';

describe("DetailViewContext", () => {
  const state = {
    breadcrumb: [],
    fileIndexItem: newIFileIndexItem(),
    relativeObjects: {} as IRelativeObjects,
    subPath: "/",
    pageType: PageType.DetailView,
    colorClassActiveList: [],
    isReadOnly: false,
    dateCache: Date.now(),
    fileHash: '1',
  } as IDetailView;
  it("update - check if item is update (append false)", () => {

    var action = { type: 'update', tags: 'tags', colorclass: 1, description: 'description', title: 'title', status: IExifStatus.Ok } as any;

    var result = detailviewReducer(state, action);

    expect(result.fileIndexItem.tags).toBe('tags');
    expect(result.fileIndexItem.colorClass).toBe(1);
    expect(result.fileIndexItem.description).toBe('description');
    expect(result.fileIndexItem.title).toBe('title');
  });

  it("update - check if orientation is updated", () => {
    var action = { type: 'update', orientation: 4 } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.orientation).toBe(4);
  });

  it("update - check if lastEdited is updated", () => {
    var action = { type: 'update', lastEdited: '2' } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.lastEdited).toBe('2');
  });

  it("update - check if dateTime is updated", () => {
    var action = { type: 'update', dateTime: '2' } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.dateTime).toBe('2');
  });

  it("update - check if fileHash is updated", () => {
    var action = { type: 'update', fileHash: '2' } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.fileHash).toBe('2');
  });

  it("append - check if tags is updated", () => {
    state.fileIndexItem.tags = ''
    var action = { type: 'append', tags: 'tags' } as any

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe(',tags');
  });

  it("remove - check if item is update", () => {
    state.fileIndexItem.tags = "!delete!"

    const action = { type: 'remove', tags: '!delete!' } as any;

    var result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe('');
  });

});
