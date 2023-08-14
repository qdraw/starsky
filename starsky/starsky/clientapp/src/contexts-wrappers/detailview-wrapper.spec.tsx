import { render } from "@testing-library/react";
import * as DetailView from "../containers/detailview/detailview";
import * as useDetailViewContext from "../contexts/detailview-context";
import { mountReactHook } from "../hooks/___tests___/test-hook";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { IDetailView, newDetailView } from "../interfaces/IDetailView";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import DetailViewWrapper, {
  DetailViewEventListenerUseEffect
} from "./detailview-wrapper";

describe("DetailViewWrapper", () => {
  it("renders", () => {
    render(<DetailViewWrapper {...newDetailView()} />);
  });

  describe("with mount", () => {
    it("check if DetailView is mounted", () => {
      const args = { ...newDetailView() } as IDetailView;
      const detailView = jest
        .spyOn(DetailView, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const compontent = render(<DetailViewWrapper {...args} />);
      expect(detailView).toBeCalled();
      compontent.unmount();
    });

    it("check if dispatch is called", () => {
      const contextValues = {
        state: { fileIndexItem: newIFileIndexItem() },
        dispatch: jest.fn()
      } as any;

      jest
        .spyOn(useDetailViewContext, "useDetailViewContext")
        .mockImplementationOnce(() => contextValues as any)
        .mockImplementationOnce(() => contextValues as any)
        .mockImplementationOnce(() => contextValues as any);

      const args = {
        ...newDetailView(),
        fileIndexItem: newIFileIndexItem()
      } as IDetailView;

      const detailView = jest
        .spyOn(DetailView, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });

      const compontent = render(<DetailViewWrapper {...args} />);

      expect(contextValues.dispatch).toBeCalled();
      expect(detailView).toBeCalled();

      compontent.unmount();
    });
  });

  describe("no context", () => {
    it("[detail view] No context if used", () => {
      const contextValues = {
        state: null,
        dispatch: jest.fn()
      } as any;

      jest
        .spyOn(useDetailViewContext, "useDetailViewContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues as any);

      const args = { ...newDetailView() } as IDetailView;
      const compontent = render(<DetailViewWrapper {...args} />);

      expect(compontent.container.innerHTML).toBe("");
      compontent.unmount();
    });
  });

  describe("DetailViewEventListenerUseEffect", () => {
    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      delete window.location;
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      window.location = {
        search: "/?f=/test.jpg"
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("Check if event is received", () => {
      const dispatch = jest.fn();
      document.body.innerHTML = "";
      const result = mountReactHook(DetailViewEventListenerUseEffect, [
        dispatch
      ]);

      const detail = {
        data: [
          {
            ...newIFileIndexItem()
            // should ignore this one
          },
          {
            colorclass: undefined,
            ...newIFileIndexItem(),
            filePath: "/test.jpg",
            type: "update"
          }
        ]
      };
      const event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith(detail.data[1]);

      const element = result.componentMount as any;
      element.unmount();
    });
  });
});
