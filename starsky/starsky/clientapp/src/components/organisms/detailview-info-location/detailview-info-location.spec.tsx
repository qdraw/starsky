import { render } from "@testing-library/react";
import DetailViewInfoLocation from "./detailview-info-location";

describe("ModalGeo", () => {


    it("renders", () => {
      render(
        <DetailViewInfoLocation
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={() => {}}
          latitude={51}
          longitude={3}
          isFormEnabled={false}
        ></ModalGeo>
      );
    });
});
