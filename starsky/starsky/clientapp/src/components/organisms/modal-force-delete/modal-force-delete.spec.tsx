import { shallow } from "enzyme";
import { newIArchive } from "../../../interfaces/IArchive";
import ModalForceDelete from "./modal-force-delete";

describe("ModalForceDelete", () => {
  it("renders", () => {
    shallow(
      <ModalForceDelete
        isOpen={true}
        handleExit={() => {}}
        dispatch={jest.fn()}
        select={[]}
        setIsLoading={jest.fn()}
        setSelect={jest.fn()}
        state={newIArchive()}
      ></ModalForceDelete>
    );
  });
});
