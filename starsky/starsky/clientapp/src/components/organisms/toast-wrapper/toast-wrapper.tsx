import { useEffect, useState } from "react";
import { ToastOption } from "../../../providers/toast-config";
import { ToastItem, useToast } from "../../../providers/toast-provider";

export const ToastWrapper = (): JSX.Element => {
  const toast = useToast();
  const [toastItem, setToastItem] = useState<ToastOption | undefined>(
    undefined
  );

  useEffect(() => {
    const activeToast: ToastItem | undefined = toast.items?.find(
      (toast) => toast?.show
    );
    setToastItem(activeToast?.content || undefined);
  }, [toast]);

  const hideToast = (id: string): void => toast.hide(id);

  return <Toast {...toastItem} onRemove={(id: string) => hideToast(id)} />;
};
