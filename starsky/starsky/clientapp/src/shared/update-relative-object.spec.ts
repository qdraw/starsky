import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import { newDetailView } from "../interfaces/IDetailView";
import * as FetchGet from "./fetch-get";
import { UpdateRelativeObject } from "./update-relative-object";

describe("UpdateRelativeObject", () => {
  it("status failing should reject", (done) => {
    const test = jest.fn();
    const connectionDefault2: IConnectionDefault = {
      statusCode: 400,
      data: "key"
    };
    const mockIConnectionDefault2: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault2);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault2);

    new UpdateRelativeObject()
      .Update({ ...newDetailView(), subPath: "/test" }, true, "/?t=test", test)
      .then((status) => {
        console.log("--should not display status");
        console.log(status);
      })
      .catch(() => {
        done();
      });
  });

  it("status 200 should accept", (done) => {
    const test = jest.fn();
    const connectionDefault2: IConnectionDefault = {
      statusCode: 200,
      data: "key"
    };
    const mockIConnectionDefault2: Promise<IConnectionDefault> =
      Promise.resolve(connectionDefault2);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault2);

    new UpdateRelativeObject()
      .Update(newDetailView(), true, "/?t=test", test)
      .then(() => {
        done();
      });
  });

  it("FetchGet rejects", (done) => {
    const test = jest.fn();
    const connectionDefault2: IConnectionDefault = {
      statusCode: 200,
      data: "key"
    };
    const mockIConnectionDefault2: Promise<IConnectionDefault> =
      Promise.reject(connectionDefault2);

    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockIConnectionDefault2);

    new UpdateRelativeObject()
      .Update({ ...newDetailView(), subPath: "/test" }, true, "/?t=test", test)
      .then((status) => {
        console.log("--should not display status");
        console.log(status);
      })
      .catch(() => {
        done();
      });
  });
});
