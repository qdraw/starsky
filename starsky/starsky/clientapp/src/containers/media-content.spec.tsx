import { render } from "@testing-library/react";
import React from "react";
import * as Notification from "../components/atoms/notification/notification";
import * as HealthStatusError from "../components/molecules/health-status-error/health-status-error";
import * as useSockets from "../hooks/realtime/use-sockets";
import * as useFileList from "../hooks/use-filelist";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import MediaContent from "./media-content";

describe("MediaContent", () => {
  it("renders", () => {
    render(<MediaContent />);
  });
  it("application failed", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, "default").mockImplementationOnce(() => {
      return null;
    });

    jest.spyOn(useSockets, "default").mockImplementationOnce(() => {
      return {} as useSockets.IUseSockets;
    });

    // use ==> import * as HealthStatusError from '../components/health-status-error';
    jest.spyOn(HealthStatusError, "default").mockImplementationOnce(() => null);

    const component = render(<MediaContent />);
    expect(component.html()).toBe(
      "<br>The application has failed. Please reload it to try it again"
    );
    component.unmount();
  });

  it("when callback it should close issue", () => {
    jest.spyOn(useFileList, "default").mockImplementationOnce(() => {
      return {
        archive: { ...newIArchive() },
        pageType: PageType.Loading
      } as any;
    });

    // import * as useSockets from "../hooks/realtime/use-sockets";
    const setShowSocketErrorSpy = jest.fn();
    jest.spyOn(useSockets, "default").mockImplementationOnce(() => {
      return {
        showSocketError: true,
        setShowSocketError: setShowSocketErrorSpy
      } as useSockets.IUseSockets;
    });

    jest.spyOn(HealthStatusError, "default").mockImplementationOnce(() => null);

    jest.spyOn(Notification, "default").mockImplementationOnce((props) => {
      if (props.callback) {
        props.callback();
      }
      return <div className="test">testung</div>;
    });

    const component = render(<MediaContent />);

    expect(setShowSocketErrorSpy).toBeCalled();
    component.unmount();
  });
});
