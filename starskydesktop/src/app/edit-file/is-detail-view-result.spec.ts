import { IGetNetRequestResponse } from "../net-request/get-net-request";
import { IsDetailViewResult } from "./is-detail-view-result";

describe("IsDetailViewResult", () => {
  it("should return true for valid result", () => {
    const validResult: IGetNetRequestResponse = {
      statusCode: 200,
      data: {
        fileIndexItem: {
          status: "Ok",
          collectionPaths: [],
          sidecarExtensionsList: [],
        },
      },
    };
    expect(IsDetailViewResult(validResult)).toBe(true);
  });

  it("should return false if statusCode is not 200", () => {
    const invalidStatusCode: IGetNetRequestResponse = {
      statusCode: 404,
      data: {
        fileIndexItem: {
          status: "Ok",
          collectionPaths: [],
          sidecarExtensionsList: [],
        },
      },
    };
    expect(IsDetailViewResult(invalidStatusCode)).toBe(false);
  });

  it("should return false if any required property is missing", () => {
    const missingData: IGetNetRequestResponse = {
      statusCode: 200,
      data: undefined,
    };
    const missingFileIndexItem: IGetNetRequestResponse = {
      statusCode: 200,
      data: {},
    };
    const missingStatus: IGetNetRequestResponse = {
      statusCode: 200,
      data: {
        fileIndexItem: {},
      },
    };
    expect(IsDetailViewResult(missingData)).toBeUndefined();
    expect(IsDetailViewResult(missingFileIndexItem)).toBeUndefined();
    expect(IsDetailViewResult(missingStatus)).toBeUndefined();
  });

  it('should return false if status is not "Ok", "OkAndSame", or "Default"', () => {
    const invalidStatus: IGetNetRequestResponse = {
      statusCode: 200,
      data: {
        fileIndexItem: {
          status: "InvalidStatus",
          collectionPaths: [],
          sidecarExtensionsList: [],
        },
      },
    };
    expect(IsDetailViewResult(invalidStatus)).toBe(false);
  });
});
