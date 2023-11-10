import { render } from "@testing-library/react";
import * as DropArea from "../../../atoms/drop-area/drop-area";
import { UploadMenuItem } from "./upload-menu-item";

describe("MenuArchive", () => {
  it("renders", () => {
    render(
      <UploadMenuItem
        readOnly={true}
        setDropAreaUploadFilesList={() => {}}
        dispatch={() => {}}
        state={{} as any}
      />
    );
  });

  describe("with Context", () => {
    it("test callback", () => {
      const dropAreaSpy = jest
        .spyOn(DropArea, "default")
        .mockImplementation((props) => {
          if (props.callback) {
            props.callback([]);
          }
          return <div></div>;
        });

      const dispatch = jest.fn();
      const component = render(
        <UploadMenuItem
          readOnly={false}
          setDropAreaUploadFilesList={() => {}}
          dispatch={dispatch}
          state={{} as any}
        />
      );

      expect(dropAreaSpy).toBeCalled();
      expect(dispatch).toBeCalled();

      component.unmount();
    });
  });
});
