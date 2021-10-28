import { render } from "@testing-library/react";
import React from "react";
import * as ModalPublish from "./modal-publish";
import ModalPublishToggleWrapper from "./modal-publish-toggle-wrapper";

describe("ModalPublish", () => {
  it("renders", () => {
    render(
      <ModalPublishToggleWrapper
        select={["/"]}
        stateFileIndexItems={[]}
        isModalPublishOpen={true}
        setModalPublishOpen={() => {}}
      ></ModalPublishToggleWrapper>
    );
  });
  it("should Not pass undefined", () => {
    const modalPublishSpy = jest
      .spyOn(ModalPublish, "default")
      .mockImplementationOnce(() => <></>);
    const component = render(
      <ModalPublishToggleWrapper
        select={undefined}
        stateFileIndexItems={[{ fileName: undefined } as any]}
        isModalPublishOpen={true}
        setModalPublishOpen={() => {}}
      ></ModalPublishToggleWrapper>
    );
    expect(modalPublishSpy).toBeCalledTimes(0);
    component.unmount();
  });
  it("pass value", () => {
    const modalPublishSpy = jest
      .spyOn(ModalPublish, "default")
      .mockImplementationOnce(() => <></>);
    const component = render(
      <ModalPublishToggleWrapper
        select={["/"]}
        stateFileIndexItems={[{ fileName: undefined } as any]}
        isModalPublishOpen={true}
        setModalPublishOpen={() => {}}
      ></ModalPublishToggleWrapper>
    );
    expect(modalPublishSpy).toBeCalledTimes(1);
    component.unmount();
  });
});
