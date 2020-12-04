import { mount } from "enzyme";
import React from "react";
import * as ApplicationException from "../components/organisms/application-exception/application-exception";
import * as ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import * as useSearchList from "../hooks/use-searchlist";
import { ISearchList } from "../hooks/use-searchlist";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import TrashPage from "./trash-page";

describe("TrashPage", () => {
  it("default error case", () => {
    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {} as ISearchList;
    });

    var error = mount(<TrashPage>t</TrashPage>);
    expect(error.text()).toBe("Something went wrong");
  });

  it("check if context is called", () => {
    var contextSpy = jest
      .spyOn(ArchiveContextWrapper, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        archive: newIArchive(),
        pageType: PageType.Trash
      } as ISearchList;
    });

    var trashPage = mount(<TrashPage>t</TrashPage>);

    expect(contextSpy).toBeCalled();

    trashPage.unmount();
  });

  it("Internal Error null", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => null as any);

    const component = mount(<TrashPage>t</TrashPage>);
    expect(component.html()).toBe("Something went wrong");

    component.unmount();
  });

  it("Internal Error plain object", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => ({} as any));

    const component = mount(<TrashPage>t</TrashPage>);
    expect(component.html()).toBe("Something went wrong");

    component.unmount();
  });

  it("App exception", () => {
    const applicationExceptionSpy = jest
      .spyOn(ApplicationException, "default")
      .mockImplementationOnce(() => null);

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        pageType: PageType.ApplicationException
      } as any;
    });

    const component = mount(<TrashPage>t</TrashPage>);

    expect(applicationExceptionSpy).toBeCalled();

    component.unmount();
  });
});
