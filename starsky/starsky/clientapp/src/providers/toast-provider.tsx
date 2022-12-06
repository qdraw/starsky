/* eslint-disable react-hooks/exhaustive-deps */

import {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useState
} from "react";
import { ToastConfig, ToastOption, ToastVariant } from "./toast-config";

interface ToastProviderValue {
  items: ToastItem[];
  register: (id: string) => void;
  open: (options: ToastOption) => void;
  success: (id: string, message?: string) => void;
  fail: (id: string, message?: string) => void;
  hide: (id: string) => void;
}

export interface ToastItem {
  show: boolean;
  content: ToastOption;
}

const ToastContext = createContext<ToastProviderValue>({
  items: [],
  register: () => {},
  hide: () => {},
  success: () => {},
  fail: () => {},
  open: () => {}
});

export function useToast(id?: string): ToastProviderValue {
  const toast = useContext(ToastContext);

  if (id) {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    useEffect(() => {
      toast.register(id); // register toast if id is supplied
    }, []);
  }

  return toast;
}

/** This provider contains all the registered toasts, either active or inactive */
export const ToastProvider = ({
  children
}: {
  children: ReactNode;
}): JSX.Element => {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const getActiveToast = (items?: ToastItem[]): ToastOption =>
    (items || toasts)?.find((toast) => toast.show)!.content;
  const isRegistered = (id: string, items?: ToastItem[]): boolean =>
    !!(items || toasts)?.find((toast) => toast.content.id === id);
  const shouldAutoDismiss = (toast: ToastOption): boolean =>
    !toast.actionText && !toast.onAction;
  const delay = async (timeout: number): Promise<void> =>
    new Promise((res) => setTimeout(res, timeout));

  /** Adds the toast to the list of toasts */
  const register = (id: string): void => {
    if (isRegistered(id)) return;
    const newItem: ToastItem = { content: { id, message: "" }, show: false };

    setToasts((current: ToastItem[]) => {
      return isRegistered(id, current) ? current : [...current, newItem];
    });
  };

  /** Activates the selected toast */
  const open = (options: ToastOption): void => {
    const openToast = (): void => {
      setToasts((current: ToastItem[]) => {
        if (!current || current.length === 0) return [];

        const selectedItem = current.find(
          (toast) => toast.content.id === options.id
        );
        if (!selectedItem || selectedItem.show) return current;
        selectedItem.content = options;
        selectedItem.show = true;

        const newCurrent = current.filter(
          (item) => item.content.id !== options.id
        );
        return [...newCurrent, selectedItem];
      });
    };

    const activeToastContent: ToastOption = getActiveToast(toasts);
    const toastIsAlreadyOpen: boolean = activeToastContent?.id === options.id;
    if (toastIsAlreadyOpen) return;

    if (activeToastContent) {
      hide(activeToastContent.id);
      setTimeout(() => {
        openToast();
      }, 250);
    } else {
      openToast();
    }

    if (shouldAutoDismiss(options))
      delay(ToastConfig.AUTO_DISMISS_TIME).then(() => hide(options.id));
  };

  const success = (id: string, message?: string): void =>
    updateToast(id, ToastVariant.SUCCESS, message);
  const fail = (id: string, message?: string): void =>
    updateToast(id, ToastVariant.ERROR, message);

  /** Updates to toast to success or error variant, and if indicated its message */
  const updateToast = (
    id: string,
    variant: ToastVariant,
    message?: string
  ): void => {
    setToasts((current: ToastItem[]) => {
      const activeToastContent: ToastOption = getActiveToast(current);
      const hasSameId: boolean = activeToastContent?.id === id;
      if (!activeToastContent || !hasSameId) return current;

      const content: ToastOption = {
        ...activeToastContent,
        variant,
        ...(message && { message })
      };

      const newList = [...current];
      newList.find((toast) => toast.show === true)!.content = content;
      return newList;
    });
  };

  /** Deactivates the active toast (but keeps it in the toasts array) */
  const hide = (id: string): void => {
    setToasts((current: ToastItem[]) => {
      if (getActiveToast(current)?.id !== id) return current;
      const newArray = [...current];
      newArray.forEach((toast) => (toast.show = false));
      return newArray;
    });
  };

  const providerValue: ToastProviderValue = {
    items: toasts,
    register,
    open,
    success,
    fail,
    hide
  };

  return (
    <ToastContext.Provider value={providerValue}>
      {children}
    </ToastContext.Provider>
  );
};
