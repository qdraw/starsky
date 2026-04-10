import ModalTimezoneShift from "./modal-timezone-shift";

const meta = {
  title: "components/organisms/modal-timezone-shift",
  component: ModalTimezoneShift,
  parameters: {
    layout: "centered"
  },
  tags: ["autodocs"]
};

export default meta;

export const Default = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    select: ["/test/photo1.jpg", "/test/photo2.jpg", "/test/photo3.jpg"],
    collections: true
  }
};

export const SingleFile = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    select: ["/test/photo1.jpg"],
    collections: true
  }
};

export const ManyFiles = {
  args: {
    isOpen: true,
    handleExit: () => console.log("Exit clicked"),
    select: Array.from({ length: 24 }, (_, i) => `/test/photo${i + 1}.jpg`),
    collections: true
  }
};

export const Closed = {
  args: {
    isOpen: false,
    handleExit: () => console.log("Exit clicked"),
    select: ["/test/photo1.jpg"],
    collections: true
  }
};
