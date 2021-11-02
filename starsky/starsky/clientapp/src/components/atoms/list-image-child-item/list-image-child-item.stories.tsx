import { storiesOf } from "@storybook/react";
import React from "react";
import {
  IFileIndexItem,
  ImageFormat
} from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "./list-image-child-item";

storiesOf("components/molecules/list-image-child-item", module).add(
  "default",
  () => {
    const exampleItems = [
      {
        fileName: "test.jpg",
        filePath: "/test.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: "2021-01-30T16:26:43.776883",
        size: 1975258
      },
      {
        fileName: "test2.png",
        filePath: "/test2.png",
        isDirectory: false,
        imageFormat: ImageFormat.png,
        lastEdited: "2020-01-30T10:26:43.776883",
        size: 43994
      },
      {
        fileName: "test2.jpg",
        filePath: "/test2.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: "2020-01-30T16:26:43.776883",
        size: 43994
      },
      {
        fileName: "test2.tiff",
        filePath: "/test2.tiff",
        isDirectory: false,
        imageFormat: ImageFormat.tiff,
        lastEdited: "2020-01-30T16:26:43.776883",
        size: 43994
      },
      {
        fileName: "20210101_120000_DSC000001_e.jpg",
        filePath: "/20210101_120000_DSC000001_e.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: "2020-01-30T16:26:43.776883",
        size: 2197532
      },
      {
        fileName: "very_long_name_that_is_not_helpful_but_for_test_only.jpg",
        filePath: "/very_long_name_that_is_not_helpful_but_for_test_only.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: "2020-01-30T16:26:43.776883",
        size: 2275263
      },
      {
        fileName: "test",
        filePath: "/test",
        isDirectory: true,
        imageFormat: ImageFormat.unknown,
        lastEdited: "2020-01-30T16:26:43.776883",
        size: 0
      },
      {
        fileName: "now.jpg",
        filePath: "/now.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: new Date().toISOString(),
        size: 2317897
      },
      {
        fileName: "1_day_ago.jpg",
        filePath: "/1_day_ago.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: new Date(new Date().valueOf() - 1 * 86400000).toISOString(),
        size: 43994
      },
      {
        fileName: "1_hour_ago.jpg",
        filePath: "/1_hour_ago.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: new Date(new Date().valueOf() - 1 * 3600000).toISOString(),
        size: 43994
      },
      {
        fileName: "6_minutes_ago.jpg",
        filePath: "/6_minutes_ago.jpg",
        isDirectory: false,
        imageFormat: ImageFormat.jpg,
        lastEdited: new Date(new Date().valueOf() - 1 * 360000).toISOString(),
        size: 43994
      }
    ] as IFileIndexItem[];

    return (
      <div className={"folder"}>
        {/* EXAMPLE STORY */}
        <div className={"list-image-box box--view"}>
          {exampleItems.map((item) => (
            <div className={"box-content colorclass--0 isDirectory-true"}>
              <ListImageChildItem
                {...item}
                key={item.fileName + item.lastEdited + item.colorClass}
              />
            </div>
          ))}
        </div>
      </div>
    );
  }
);
