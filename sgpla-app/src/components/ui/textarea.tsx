"use client";

import * as React from "react";

import { cn } from "@/lib/utils";
import { useTextFieldContext } from "@/components/text-field-context";

function Textarea({
  className,
  id,
  required,
  ...props
}: React.ComponentProps<"textarea">) {
  const textField = useTextFieldContext();

  return (
    <textarea
      data-slot="textarea"
      id={id ?? textField?.id}
      required={required ?? textField?.required}
      className={cn(
        "flex field-sizing-content min-h-16 w-full resize-none rounded-2xl border border-transparent bg-input/50 px-3 py-3 text-base transition-[color,box-shadow,background-color] outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20 md:text-sm",
        className,
      )}
      {...props}
    />
  );
}

export { Textarea };
