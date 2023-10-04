import "core-js/features/array/some"; // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/some
import { IArchive, newIArchive, SortType } from "../interfaces/IArchive";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import {
  IFileIndexItem,
  ImageFormat,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import ArrayHelper from "../shared/array-helper";
import { FileListCache } from "../shared/filelist-cache";
import {
  ArchiveAction,
  archiveReducer,
  filterSidecarItems
} from "./archive-context";

describe("ArchiveContext", () => {
  it("filterSidecarItems removes sidecar files when collections are enabled", () => {
    const actionAdd: IFileIndexItem[] = [
      {
        filePath: "/path/to/image.xmp",
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem,
      {
        filePath: "/path/to/image2.jpg",
        imageFormat: ImageFormat.jpg
      } as IFileIndexItem
    ];
    const fileIndexItems: IFileIndexItem[] = [
      {
        filePath: "/path/to/image.xmp",
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem,
      {
        filePath: "/path/to/image2.jpg",
        imageFormat: ImageFormat.jpg
      } as IFileIndexItem
    ];
    const state: IArchiveProps = {
      collections: true
    } as IArchiveProps;

    const result = filterSidecarItems(actionAdd, fileIndexItems, state);

    expect(result).toEqual([
      {
        filePath: "/path/to/image2.jpg",
        imageFormat: ImageFormat.jpg
      }
    ]);
  });

  it("remove-folder - folder should be gone", () => {
    const state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        },
        {
          filePath: "/test.jpg"
        }
      ]
    } as IArchiveProps;

    const action = { type: "remove-folder" } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("rename-folder - path should be changed", () => {
    const state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        },
        {
          filePath: "/test.jpg"
        }
      ],
      subPath: "/test"
    } as IArchiveProps;

    const action = { type: "rename-folder", path: "/test2" } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.subPath).toBe("/test2");
  });

  it("force-reset - it should not add duplicate content", () => {
    const state = {
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
    const action = { type: "force-reset", payload: state } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test.jpg");
  });

  it("force-reset - should update cache", () => {
    const state = {
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
    const action = { type: "force-reset", payload: state } as ArchiveAction;

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
    const state = {
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
      .mockReset()
      .mockImplementationOnce(() => {});

    // fullPath input
    const action = { type: "force-reset", payload: state } as ArchiveAction;

    archiveReducer(state, action);

    expect(cacheSetObjectSpy).toBeCalledTimes(0);
    cacheSetObjectSpy.mockReset();
  });

  it("set - it should ignore when fileIndexItem is undefined", () => {
    const action = { type: "set", payload: {} } as ArchiveAction;
    const result = archiveReducer({} as any, action);

    expect(result.fileIndexItems).toBeUndefined();
  });

  it("set - it should not add duplicate content", () => {
    const state = {
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
    const action = { type: "set", payload: state } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test.jpg");
  });

  it("set - should not default default sort when is PageType.Archive", () => {
    const state = {
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
    const action = { type: "set", payload: state } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/a.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__first.mp4");
  });

  it("set - should sort imageFormat when is PageType.Archive", () => {
    const state = {
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
    const action = { type: "set", payload: state } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/a.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__first.mp4");
  });

  it("set - should ignore when is PageType.Search", () => {
    const state = {
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
    const action = { type: "set", payload: state } as ArchiveAction;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].filePath).toBe("/first.jpg");
    expect(result.fileIndexItems[1].filePath).toBe("/__not_first.mp4");
  });

  it("remove - check if item is removed", () => {
    const state = {
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
    const action = { type: "remove", toRemoveFileList: ["/test.jpg"] } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe("/test1.jpg");
  });

  it("remove - last colorClassUsage is removed", () => {
    const state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        }
      ],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    // fullpath input
    const action = { type: "remove", toRemoveFileList: ["/test.jpg"] } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
    expect(result.colorClassUsage).toStrictEqual([]);
  });

  it("add - undefined", () => {
    const state = { fileIndexItems: newIFileIndexItemArray() } as IArchiveProps;

    // fullpath input
    const action = { type: "add", add: undefined } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("add - last colorClassUsage is removed", () => {
    const state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg"
        }
      ],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.jpg",
        status: IExifStatus.Deleted // <- say its deleted
      }
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
    expect(result.colorClassUsage).toStrictEqual([]);
  });

  it("add - overwrite tags", () => {
    const state = {
      fileIndexItems: [
        {
          filePath: "/test.jpg",
          tags: "init"
        }
      ] as IFileIndexItem[],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.jpg",
        status: IExifStatus.Ok,
        tags: "test",
        imageWidth: 150
      } as IFileIndexItem
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0]).toBe(add[0]);
  });

  it("add - ignore xmp file when collections is true", () => {
    const state = {
      collections: true,
      fileIndexItems: [] as IFileIndexItem[],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.xmp",
        status: IExifStatus.Ok,
        tags: "test",
        imageWidth: 150,
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("add - add xmp file when collections is false", () => {
    const state = {
      collections: false,
      fileIndexItems: [] as IFileIndexItem[],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.xmp",
        status: IExifStatus.Ok,
        tags: "test",
        imageWidth: 150,
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0]).toBe(add[0]);
  });

  it("add - ignore meta_json file when collections is undefined", () => {
    const state = {
      collections: undefined,
      fileIndexItems: [] as IFileIndexItem[],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.meta.json",
        status: IExifStatus.Ok,
        tags: "test",
        imageWidth: 150,
        imageFormat: ImageFormat.meta_json
      } as IFileIndexItem
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("add - add meta_json file when collections is false", () => {
    const state = {
      collections: false,
      fileIndexItems: [] as IFileIndexItem[],
      colorClassUsage: [1, 2]
    } as IArchiveProps;

    const add = [
      {
        filePath: "/test.meta.json",
        status: IExifStatus.Ok,
        tags: "test",
        imageWidth: 150,
        imageFormat: ImageFormat.meta_json
      } as IFileIndexItem
    ];

    // fullpath input
    const action = { type: "add", add } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0]).toBe(add[0]);
  });

  it("update - check if item is update (append false)", () => {
    const state = {
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
    const action = {
      type: "update",
      fileHash: "1",
      tags: "tags",
      colorclass: 1,
      description: "description",
      title: "title",
      append: false,
      select: ["test.jpg"]
    } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].tags).toBe("tags");
    expect(result.fileIndexItems[0].fileHash).toBe("1");
    expect(result.fileIndexItems[0].colorClass).toBe(1);
    expect(result.fileIndexItems[0].description).toBe("description");
    expect(result.fileIndexItems[0].title).toBe("title");
  });

  it("update - check if item is update when not found (append false)", () => {
    const state = {
      fileIndexItems: [
        {
          fileName: "test.jpg",
          tags: "tags1",
          description: "description1",
          title: "title1"
        }
      ]
    } as IArchiveProps;
    const action = {
      type: "update",
      tags: "tags",
      description: "description",
      title: "title",
      append: true,
      select: ["test.jpg", "notfound.jpg"]
    } as any;

    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].tags).toBe("tags1, tags");
    expect(result.fileIndexItems[0].description).toBe(
      "description1description"
    );
    expect(result.fileIndexItems[0].title).toBe("title1title");
  });

  it("update - colorclass check usage", () => {
    const state = {
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

    const action = {
      type: "update",
      colorClass: 2,
      select: ["test.jpg"]
    } as any;

    const result = archiveReducer(state, action);

    expect(result.colorClassUsage.length).toBe(2);
    expect(result.colorClassUsage[0]).toBe(1);
    expect(result.colorClassUsage[1]).toBe(2);
  });

  it("update - colorclass 2", () => {
    const state = {
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

    const action = {
      type: "update",
      colorClass: 3,
      select: ["test.jpg"]
    } as any;

    const result = archiveReducer(state, action);

    expect(result.colorClassUsage.length).toBe(1);
    expect(result.colorClassUsage[0]).toBe(2);
  });

  it("add -- check when added the ColorClassUsage field is updated", () => {
    // current state
    const state = {
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

    const add = [
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

    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

    expect(result.colorClassUsage).toStrictEqual([2, 0]);
  });

  it("add -- and check if is orderd", () => {
    // current state
    const state = {
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
    const add = [
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
    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

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

  it("add -- remove a collection item", () => {
    // current state
    const state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok
        },
        {
          fileName: "test0.mp4",
          filePath: "/test0.mp4",
          status: IExifStatus.Ok
        }
      ],
      collections: true
    } as IArchiveProps;

    // to add/remove this
    const add = [
      {
        fileName: "test0.mp4",
        filePath: "/test0.mp4",
        status: IExifStatus.Deleted
      }
    ];
    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "test0.jpg",
        filePath: "/test0.jpg",
        status: IExifStatus.Ok,
        imageFormat: ImageFormat.unknown
      }
    ]);
  });

  it("add -- implicit delete", () => {
    console.log("-implicit delete");

    // current state
    const state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          fileCollectionName: "test0",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok
        },
        {
          fileName: "test1.jpg",
          fileCollectionName: "test1",
          filePath: "/test1.jpg",
          status: IExifStatus.Ok
        }
      ],
      collections: true
    } as IArchiveProps;

    // to add/remove this
    const add = [
      {
        fileName: "test0.jpg",
        filePath: "/test0.jpg",
        status: IExifStatus.Deleted
      }
    ];
    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

    console.log("<--");

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "test1.jpg",
        fileCollectionName: "test1",
        filePath: "/test1.jpg",
        status: IExifStatus.Ok
      }
    ]);
  });

  it("add -- change order for raw files", () => {
    console.log("-change order for raw files");

    // current state
    const state = {
      fileIndexItems: newIFileIndexItemArray(),
      collections: true
    } as IArchiveProps;

    // to add/remove this
    const add = [
      {
        fileName: "test0.arw",
        fileCollectionName: "test0",
        filePath: "/test0.arw",
        imageFormat: ImageFormat.tiff,
        status: IExifStatus.Ok
      },
      {
        fileName: "test0.jpg",
        fileCollectionName: "test0",
        filePath: "/test0.jpg",
        imageFormat: ImageFormat.jpg,
        status: IExifStatus.Ok
      }
    ];

    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

    console.log("<--");

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: "test0.jpg",
        fileCollectionName: "test0",
        filePath: "/test0.jpg",
        imageFormat: "jpg",
        status: IExifStatus.Ok
      }
    ]);
  });

  it("add -- and check if is ordered", () => {
    // current state
    const state = {
      fileIndexItems: [
        {
          fileName: "2018.01.01.17.00.01.jpg",
          filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    // to add this
    const add = [
      {
        fileName: "__20180101170001.jpg",
        filePath: "/__starsky/01-dif/__20180101170001.jpg",
        status: IExifStatus.Ok
      }
    ];
    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

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
    const state = {
      fileIndexItems: [],
      colorClassActiveList: [2, 4]
    };

    // to add this
    const add = [
      {
        fileName: "__20180101170001.jpg",
        filePath: "/__starsky/01-dif/__20180101170001.jpg",
        status: IExifStatus.Ok,
        colorClass: 1
      }
    ];
    const action = { type: "add", add } as any;
    const result = archiveReducer(state as any, action);

    expect(result.fileIndexItems.length).toBe(0);
  });

  it("add -- duplicate", () => {
    // current state
    const state = {
      fileIndexItems: [
        {
          fileName: "2018.01.01.17.00.01.jpg",
          filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    // to add this
    const add = [
      {
        fileName: "2018.01.01.17.00.01.jpg",
        filePath: "/__starsky/01-dif/2018.01.01.17.00.01.jpg",
        status: IExifStatus.Ok,
        tags: "updated"
      }
    ];
    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

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
    const state = {
      fileIndexItems: [
        {
          fileName: "test0.jpg",
          filePath: "/test0.jpg",
          status: IExifStatus.Ok
        }
      ]
    } as IArchiveProps;

    const action = { type: "add", add: state.fileIndexItems[0] } as any;
    const result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
  });

  it("add -- duplicate (in collections mode true)", () => {
    // current state
    const state = {
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

    const add = {
      fileName: "test0.dng",
      filePath: "/test0.dng",
      fileCollectionName: "test0",
      status: IExifStatus.Ok
    };

    const uniqueResultsSpy = jest
      .spyOn(ArrayHelper.prototype, "UniqueResults")
      .mockImplementationOnce(() => [add]);

    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

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
    const state = {
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

    const add = {
      fileName: "test0.dng",
      filePath: "/test0.dng",
      fileCollectionName: "test0",
      status: IExifStatus.Ok
    };

    jest.spyOn(ArrayHelper.prototype, "UniqueResults").mockReset();
    const uniqueResultsSpy = jest
      .spyOn(ArrayHelper.prototype, "UniqueResults")
      .mockImplementationOnce(() => [add]);

    const action = { type: "add", add } as any;
    const result = archiveReducer(state, action);

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
