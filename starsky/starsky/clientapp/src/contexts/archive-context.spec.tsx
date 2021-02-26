import "core-js/features/array/some"; // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/some
import { IArchive, newIArchive, SortType } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { ImageFormat } from "../interfaces/IFileIndexItem";
import ArrayHelper from "../shared/array-helper";
import { FileListCache } from "../shared/filelist-cache";
import { ArchiveAction, archiveReducer } from "./archive-context";

describe("ArchiveContext", () => {
  it("force-reset - it should not add duplicate content", () => {
    var state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        },
        {
          filePath: "/test.jpg"
        }
      ]
    } as IArchiveProps;

    // fullPath input
    var action = { type: "force-reset", payload: state } as ArchiveAction;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test.jpg");
  });

  it("force-reset - should update cache", () => {
    var state = {
      pageType: PageType.Archive,
      subPath: "/",
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        }
      ]
    } as IArchiveProps;

    const cacheSetObjectSpy = jest
      .spyOn(FileListCache.prototype, "CacheSetObject")
      .mockImplementationOnce(() => {});

    // fullPath input
    var action = { type: "force-reset", payload: state } as ArchiveAction;

    archiveReducer(state, action);

    expect(cacheSetObjectSpy).toBeCalled();
    expect(cacheSetObjectSpy).toBeCalledWith(
      {
        collections: undefined,
        colorClass: undefined,
        f: "/",
        sort: undefined
      },
      {
        fileIndexItems: [{ filePath: "/test.jpg" }],
        pageType: "Archive",
        subPath: "/"
      }
    );
  });

  it("force-reset - should ignore cache due pageType", () => {
    var state = {
      pageType: PageType.Search,
      subPath: "/",
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        }
      ]
    } as IArchiveProps;

    const cacheSetObjectSpy = jest
      .spyOn(FileListCache.prototype, "CacheSetObject")
      .mockImplementationOnce(() => {});

    // fullPath input
    var action = { type: "force-reset", payload: state } as ArchiveAction;

    archiveReducer(state, action);

    expect(cacheSetObjectSpy).toBeCalledTimes(0);
    cacheSetObjectSpy.mockReset();
  });

  it("set - it should ignore when fileIndexItem is undefined", () => {
    var action = { type: "set", payload: {} } as ArchiveAction;
    var result = archiveReducer({} as any, action);

    expect(result.fileIndexItems).toBeUndefined();
  });

  it("set - it should not add duplicate content", () => {
    var state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        },
        {
          filePath: "/test.jpg"
        }
      ]
    } as IArchiveProps;

    // fullPath input
    var action = { type: "set", payload: state } as ArchiveAction;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test.jpg");
  });

  it("set - should not default default sort when is PageType.Archive", () => {
    var state = {
      fileIndexItems: [
        {
          fileName: "a.jpg",
          filePath: "/a.jpg",
          imageFormat: ImageFormat.jpg
        },
        {
          fileName: "__first.mp4",
          filePath: "/__first.mp4",
          imageFormat: ImageFormat.mp4
        }
      ],
      pageType: PageType.Archive // < - - - - - - - - -
    } as IArchiveProps;

    // fullPath input
    var action = { type: "set", payload: state } as ArchiveAction;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/a.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__first.mp4");
  });

  it("set - should sort imageFormat when is PageType.Archive", () => {
    var state = {
      fileIndexItems: [
        {
          fileName: "a.jpg",
          filePath: "/a.jpg",
          imageFormat: ImageFormat.jpg
        },
        {
          fileName: "__first.mp4",
          filePath: "/__first.mp4",
          imageFormat: ImageFormat.mp4
        }
      ],
      sort: SortType.imageFormat, // < - - - - - - - - -
      pageType: PageType.Archive // < - - - - - - - - -
    } as IArchiveProps;

    // fullPath input
    var action = { type: "set", payload: state } as ArchiveAction;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/a.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__first.mp4");
  });

  it("set - should ignore when is PageType.Search", () => {
    var state = {
      fileIndexItems: [
        {
          fileName: "first.jpg",
          filePath: "/first.jpg",
          imageFormat: ImageFormat.jpg
        },
        {
          fileName: "__not_first.mp4",
          filePath: "/__not_first.mp4",
          imageFormat: ImageFormat.mp4
        }
      ],
      pageType: PageType.Search // < - - - - - - - - -
    } as IArchiveProps;

    // fullPath input
    var action = { type: "set", payload: state } as ArchiveAction;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/first.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__not_first.mp4");
  });

  it("remove - check if item is removed", () => {
    var state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        },
        {
          filePath: "/test1.jpg"
        }
      ]
    } as IArchiveProps;

    // fullpath input
    var action = { type: "remove", toRemoveFileList: ["/test.jpg"] } as any;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test1.jpg");
  });

  it("update - check if item is update (append false)", () => {
    var state = {
      ...newIArchive(),
      fileIndexItems: [
        {
          fileName: "test.jpg"
        },
        {
          fileName: "test1.jpg"
        }
      ],
      colorClassUsage: [] as number[]
    } as IArchive;
    var action = {
      type: "update",
      fileHash: "1",
      tags: "tags",
      colorclass: 1,
      description: "description",
      title: "title",
      append: false,
      select: ["test.jpg"]
    } as any;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].tags).toBe("tags");
    expect(result.fileIndexItems[0].fileHash).toBe("1");
    expect(result.fileIndexItems[0].colorClass).toBe(1);
    expect(result.fileIndexItems[0].description).toBe("description");
    expect(result.fileIndexItems[0].title).toBe("title");
  });

  it("update - check if item is update when not found (append false)", () => {
    var state = {
      fileIndexItems: [
        {
          fileName: "test.jpg",
          tags: "tags1",
          description: "description1",
          title: "title1"
        }
      ]
    } as IArchiveProps;
    var action = {
      type: "update",
      tags: "tags",
      description: "description",
      title: "title",
      append: true,
      select: ["test.jpg", "notfound.jpg"]
    } as any;

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].tags).toBe("tags1, tags");
    expect(result.fileIndexItems[0].description).toBe(
      "description1description"
    );
    expect(result.fileIndexItems[0].title).toBe("title1title");
  });

  it("update - colorclass check usage", () => {
    var state = {
      ...newIArchive(),
      fileIndexItems: [
        {
          fileName: "test.jpg",
          tags: "tags1",
          description: "description1",
          title: "title1",
          colorClass: 1
        } as any
      ],
      colorClassUsage: [1, 2],
      colorClassActiveList: [],
      breadcrumb: [],
      collectionsCount: 0,
      pageType: PageType.ApplicationException,
      dateCache: 0,
      isReadOnly: false
    } as IArchive;

    var action = { type: "update", colorClass: 2, select: ["test.jpg"] } as any;

    var result = archiveReducer(state, action);

    expect(result.colorClassUsage.length).toBe(2);
    expect(result.colorClassUsage[0]).toBe(1);
    expect(result.colorClassUsage[1]).toBe(2);
  });

  it("update - colorclass 2", () => {
    var state = {
      ...newIArchive(),
      fileIndexItems: [
        {
          fileName: "test.jpg",
          tags: "tags1",
          description: "description1",
          title: "title1",
          colorClass: 1
        } as any
      ],
      colorClassUsage: [2],
      colorClassActiveList: [],
      breadcrumb: [],
      collectionsCount: 0,
      pageType: PageType.ApplicationException,
      dateCache: 0,
      isReadOnly: false
    } as IArchive;

    var action = { type: "update", colorClass: 3, select: ["test.jpg"] } as any;

    var result = archiveReducer(state, action);

    expect(result.colorClassUsage.length).toBe(1);
    expect(result.colorClassUsage[0]).toBe(2);
  });

  it("add -- check when added the ColorClassUsage field is updated", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok,
          colorClass: 2
        },
        {
          fileName: "test2.jpg",
          filePath: "/test2.jpg",
          status: IExifStatus.Ok,
          colorClass: 2
        }
      ]
    } as IArchiveProps;

    var add = [
      {
        fileName: "test1.jpg",
        filePath: "/test1.jpg",
        status: IExifStatus.Ok,
        colorClass: 0
      },
      {
        fileName: "test3.jpg",
        filePath: "/test3.jpg",
        status: IExifStatus.Ok,
        colorClass: undefined // <--- should ignore this one
      }
    ];

    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(result.colorClassUsage).toStrictEqual([2, 0]);
  });

  it("add -- and check if is orderd", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok
        },
        {
          fileName: "test2.jpg",
          filePath: "/test2.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    // to add this
    var add = [
      {
        fileName: "test1.jpg",
        filePath: "/test1.jpg",
        status: IExifStatus.Ok
      },
      {
        fileName: "test3.jpg",
        filePath: "/test3.jpg",
        status: IExifStatus.Ok
      }
    ];
    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(4);
    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "test0.jpg",
        filePath: "/test0.jpg",
        status: IExifStatus.Ok
      },
      {
        fileName: "test1.jpg",
        filePath: "/test1.jpg",
        status: IExifStatus.Ok
      },
      {
        fileName: "test2.jpg",
        filePath: "/test2.jpg",
        status: IExifStatus.Ok
      },
      {
        fileName: "test3.jpg",
        filePath: "/test3.jpg",
        status: IExifStatus.Ok
      }
    ]);
  });

  it("add -- and check if is ordered", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "2018.01.01.17.00.01.jpg",
          filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    // to add this
    var add = [
      {
        fileName: "__20180101170001.jpg",
        filePath: "/__starsky/01-dif/__20180101170001.jpg",
        status: IExifStatus.Ok
      }
    ];
    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);

    expect(result.fileIndexItems[0].fileName).toBe("__20180101170001.jpg");

    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "__20180101170001.jpg",
        filePath: "/__starsky/01-dif/__20180101170001.jpg",
        status: IExifStatus.Ok
      },
      {
        fileName: "2018.01.01.17.00.01.jpg",
        filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
        status: IExifStatus.Ok
      }
    ]);
  });

  it("add -- try to add item outside filter", () => {
    // current state
    var state = {
      fileIndexItems: [],
      colorClassActiveList: [2, 4]
    };

    // to add this
    var add = [
      {
        fileName: "__20180101170001.jpg",
        filePath: "/__starsky/01-dif/__20180101170001.jpg",
        status: IExifStatus.Ok,
        colorClass: 1
      }
    ];
    var action = { type: "add", add } as any;
    var result = archiveReducer(state as any, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("add -- duplicate", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "2018.01.01.17.00.01.jpg",
          filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    // to add this
    var add = [
      {
        fileName: "2018.01.01.17.00.01.jpg",
        filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
        status: IExifStatus.Ok,
        tags: "updated"
      }
    ];
    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);

    expect(result.fileIndexItems[0].fileName).toBe("2018.01.01.17.00.01.jpg");

    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "2018.01.01.17.00.01.jpg",
        filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
        status: IExifStatus.Ok,
        tags: "updated"
      }
    ]);
  });

  it("add -- duplicate 2", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    var action = { type: "add", add: state.fileIndexItems[0] } as any;
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
  });

  it("add -- duplicate (in collections mode true)", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          fileCollectionName: "test0",
          status: IExifStatus.Ok
        }
      ],
      collections: true
    } as IArchiveProps;

    var add = {
      fileName: "test0.dng",
      filePath: "/test0.dng",
      fileCollectionName: "test0",
      status: IExifStatus.Ok
    };

    var uniqueResultsSpy = jest
      .spyOn(ArrayHelper.prototype, "UniqueResults")
      .mockImplementationOnce(() => [add]);

    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(uniqueResultsSpy).toBeCalledWith(
      [
        {
          fileCollectionName: "test0",
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: "Ok"
        }
      ],
      "fileCollectionName"
    );

    // the filter does not filter in jest, but it works in the browser
    expect(result.fileIndexItems.length).toBe(1);
  });

  it("add -- duplicate (in collections mode false)", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          fileCollectionName: "test0",
          status: IExifStatus.Ok
        }
      ],
      collections: false
    } as IArchiveProps;

    var add = {
      fileName: "test0.dng",
      filePath: "/test0.dng",
      fileCollectionName: "test0",
      status: IExifStatus.Ok
    };

    jest.spyOn(ArrayHelper.prototype, "UniqueResults").mockReset();
    var uniqueResultsSpy = jest
      .spyOn(ArrayHelper.prototype, "UniqueResults")
      .mockImplementationOnce(() => [add]);

    var action = { type: "add", add } as any;
    var result = archiveReducer(state, action);

    expect(uniqueResultsSpy).toBeCalledWith(
      [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          fileCollectionName: "test0",
          status: IExifStatus.Ok
        }
      ],
      "filePath"
    );

    // the filter does not filter in jest, but it works in the browser
    expect(result.fileIndexItems.length).toBe(1);
  });
});
