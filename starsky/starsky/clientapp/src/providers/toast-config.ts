export enum ToastConfig {
  AUTO_DISMISS_TIME = 8000
}

export enum ToastVariant {
  LOADING = "loading",
  SUCCESS = "success",
  ERROR = "error"
}

export interface ToastOption {
  id: string;
  message: string;
  variant?: ToastVariant;
  actionText?: string;
  onRemove?: (id: string) => void;
  onAction?: () => void;
}
