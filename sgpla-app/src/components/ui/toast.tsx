"use client";

import { Toast as ToastPrimitive } from "@base-ui/react/toast";
import { CheckCircle2, CircleAlert, Info, LoaderCircle, X } from "lucide-react";
import { cn } from "@/lib/utils";

const toastManager = ToastPrimitive.createToastManager();

function ToastIcon({ type }: { type?: string }) {
  if (type === "success")
    return <CheckCircle2 className="size-4 text-emerald-600" />;
  if (type === "error")
    return <CircleAlert className="size-4 text-destructive" />;
  if (type === "loading")
    return <LoaderCircle className="size-4 animate-spin" />;
  return <Info className="size-4 text-primary" />;
}

function ToastList() {
  const { toasts } = ToastPrimitive.useToastManager();

  return toasts.map((toast) => (
    <ToastPrimitive.Root
      key={toast.id}
      toast={toast}
      className={cn(
        "pointer-events-auto w-full rounded-lg border border-border bg-popover p-4 text-popover-foreground shadow-lg outline-none data-[ending-style]:animate-out data-[ending-style]:fade-out-0 data-[ending-style]:slide-out-to-right-2 data-[starting-style]:animate-in data-[starting-style]:fade-in-0 data-[starting-style]:slide-in-from-right-2",
      )}
    >
      <ToastPrimitive.Content className="flex items-start gap-3">
        <ToastIcon type={toast.type} />
        <div className="min-w-0 flex-1">
          <ToastPrimitive.Title className="text-sm font-medium" />
          <ToastPrimitive.Description className="mt-1 text-sm text-muted-foreground" />
        </div>
        <ToastPrimitive.Close
          className="rounded-md p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
          aria-label="Cerrar notificación"
        >
          <X className="size-4" />
        </ToastPrimitive.Close>
      </ToastPrimitive.Content>
    </ToastPrimitive.Root>
  ));
}

export function Toaster() {
  return (
    <ToastPrimitive.Provider toastManager={toastManager}>
      <ToastPrimitive.Portal>
        <ToastPrimitive.Viewport className="fixed right-4 bottom-4 z-[100] flex w-[min(24rem,calc(100vw-2rem))] flex-col gap-3 outline-none">
          <ToastList />
        </ToastPrimitive.Viewport>
      </ToastPrimitive.Portal>
    </ToastPrimitive.Provider>
  );
}

export const toast = toastManager;
