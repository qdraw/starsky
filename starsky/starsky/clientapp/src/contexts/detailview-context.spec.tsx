import { IDetailView, IRelativeObjects, PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import { DetailViewAction, detailviewReducer } from "./detailview-context";

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
    fileHash: "1"
  } as unknown as IDetailView;
  it("update - check if item is update (append false)", () => {
    const action = {
      type: "update",
      tags: "tags",
      colorclass: 1,
      description: "description",
      title: "title",
      status: IExifStatus.Ok
    } as DetailViewAction;

    const result = detailviewReducer(state, action);

    expect(result.fileIndexItem.tags).toBe("tags");
    expect(result.fileIndexItem.colorClass).toBe(1);
    expect(result.fileIndexItem.description).toBe("description");
    expect(result.fileIndexItem.title).toBe("title");
  });

  it("update - check if orientation is updated", () => {
    const action = { type: "update", orientation: 4 } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.orientation).toBe(4);
  });

  it("update - check if lastEdited is updated", () => {
    const action = { type: "update", lastEdited: "2" } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.lastEdited).toBe("2");
  });

  it("update - check if dateTime is updated", () => {
    const action = { type: "update", dateTime: "2" } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.dateTime).toBe("2");
  });

  it("update - check if fileHash is updated", () => {
    const action = { type: "update", fileHash: "2" } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.fileHash).toBe("2");
  });

  it("append - check if tags is updated", () => {
    state.fileIndexItem.tags = "";
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "append",
      tags: "tags",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe(",tags");
  });

  it("update - check if latitude is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";
    const action = {
      type: "update",
      latitude: "3",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.latitude).toBe("3");
  });

  it("update - skip when filePath is different", () => {
    state.fileIndexItem.latitude = 2;
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      latitude: "3",
      filePath: "/diff.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.latitude).toBe(2);
    expect(result.fileIndexItem.filePath).toBe("/test.jpg");
  });

  it("update - skip when filePath is different 2", () => {
    state.fileIndexItem.latitude = 2;
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      latitude: "3",
      filePath: "/diff.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.latitude).toBe(2);
  });

  it("update - check if longitude is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";
    const action = {
      type: "update",
      longitude: "2",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.longitude).toBe("2");
  });

  it("update - check if locationCity is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      locationCity: "2",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.locationCity).toBe("2");
  });

  it("update - check if locationCountry is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      locationCountry: "2",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.locationCountry).toBe("2");
  });

  it("update - check if locationCountryCode is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      locationCountryCode: "2",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.locationCountryCode).toBe("2");
  });

  it("update - check if locationState is updated", () => {
    state.fileIndexItem.filePath = "/test.jpg";

    const action = {
      type: "update",
      locationState: "2",
      filePath: "/test.jpg"
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.locationState).toBe("2");
  });

  it("remove - check if item is update", () => {
    state.fileIndexItem.tags = "!delete!";

    const action = { type: "remove", tags: "!delete!" } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);
    expect(result.fileIndexItem.tags).toBe("");
  });

  it("reset - it should overwrite", () => {
    state.fileIndexItem.tags = "!delete!";

    const action = {
      type: "reset",
      payload: { fileIndexItem: { tags: "test" } }
    } as unknown as DetailViewAction;

    const result = detailviewReducer(state, action);

    expect(result.fileIndexItem.tags).toBe("test");
  });
});
