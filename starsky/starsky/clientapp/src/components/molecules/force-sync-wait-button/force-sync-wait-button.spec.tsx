import { render, waitFor } from "@testing-library/react";
import React from "react";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { PageType } from "../../../interfaces/IDetailView";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import ForceSyncWaitButton, {
  ForceSyncRequestNewContent
} from "./force-sync-wait-button";

describe("ForceSyncWaitButton", () => {
  it("renders", () => {
    render(
      <ForceSyncWaitButton
        historyLocationSearch={""}
        dispatch={jest.fn()}
        callback={jest.fn()}
      ></ForceSyncWaitButton>
    );
  });

  const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
    ...newIConnectionDefault(),
    data: null
  });

  it("onClick value", async () => {
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const component = render(
      <ForceSyncWaitButton
        historyLocationSearch={""}
        dispatch={jest.fn()}
        callback={jest.fn()}
      />
    );

    const forceSync = component.queryByTestId(
      "force-sync"
    ) as HTMLButtonElement;
    expect(forceSync).toBeTruthy();
    forceSync.click();

    await waitFor(() => expect(fetchPostSpy).toBeCalled());

    var urlSync = new UrlQuery().UrlSync("/");
    expect(fetchPostSpy).toBeCalledWith(urlSync, "");
  });

  const mockIConnectionData: Promise<IConnectionDefault> = Promise.resolve({
    ...newIConnectionDefault(),
    data: {
      pageType: PageType.Archive,
      fileIndexItems: [
        {
          description: "",
          fileHash: undefined,
          fileName: "",
          filePath: "/test.jpg",
          isDirectory: false,
          status: "Ok",
          tags: "",
          title: ""
        }
      ]
    },
    statusCode: 200
  });

  it("ForceSyncRequestNewContent should fetch data", () => {
    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionData);

    ForceSyncRequestNewContent({
      callback: jest.fn(),
      dispatch: jest.fn(),
      historyLocationSearch: "?f=/"
    });

    expect(fetchGetSpy).toBeCalled();

    const url = new UrlQuery().UrlIndexServerApi(
      new URLPath().StringToIUrl("?f=/")
    );

    expect(fetchGetSpy).toBeCalledWith(url);
  });

  it("ForceSyncRequestNewContent should callback & dispatch", async () => {
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionData);

    const callback = jest.fn();
    const dispatch = jest.fn();

    await ForceSyncRequestNewContent({
      callback,
      dispatch,
      historyLocationSearch: "?f=/"
    });

    expect(dispatch).toBeCalled();
  });

  it("ForceSyncRequestNewContent should when failed callback & dispatch", async () => {
    const mockIConnectionDataFailed: Promise<IConnectionDefault> =
      Promise.resolve({
        ...newIConnectionDefault(),
        statusCode: 500 // < - - - - - -
      });

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDataFailed);

    const callback = jest.fn();
    const dispatch = jest.fn();

    await ForceSyncRequestNewContent({
      callback,
      dispatch,
      historyLocationSearch: "?f=/"
    });

    expect(callback).toBeCalled();
    expect(dispatch).not.toBeCalled();
  });
});
