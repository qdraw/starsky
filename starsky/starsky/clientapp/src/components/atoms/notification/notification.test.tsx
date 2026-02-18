import { act, render, screen } from "@testing-library/react";
import Notification, { NotificationType } from "./notification";

jest.useFakeTimers();

describe("Notification autoRemoveTimeout", () => {
  it("should auto-remove after the specified timeout", () => {
    const callback = jest.fn();
    render(
      <Notification type={NotificationType.default} callback={callback} autoRemoveTimeout={1000}>
        Test notification
      </Notification>
    );
    // Notification should be in the document initially
    expect(screen.getByText("Test notification")).toBeInTheDocument();
    // Fast-forward time
    act(() => {
      jest.advanceTimersByTime(1000);
    });
    // Callback should be called after timeout
    expect(callback).toHaveBeenCalled();
  });

  it("should NOT auto-remove if autoRemoveTimeout is -1", () => {
    const callback = jest.fn();
    render(
      <Notification type={NotificationType.default} callback={callback} autoRemoveTimeout={-1}>
        Persistent notification
      </Notification>
    );
    // Notification should be in the document
    expect(screen.getByText("Persistent notification")).toBeInTheDocument();
    // Fast-forward time
    act(() => {
      jest.advanceTimersByTime(5000);
    });
    // Callback should NOT be called
    expect(callback).not.toHaveBeenCalled();
  });
});
