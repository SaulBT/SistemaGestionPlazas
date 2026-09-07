"use client";

import * as React from "react";

import { cn } from "@/lib/utils";
import { useTextFieldContext } from "@/components/text-field-context";

const Label = React.forwardRef<HTMLLabelElement, React.ComponentProps<"label">>(
  ({ className, htmlFor, ...props }, ref) => {
    const textField = useTextFieldContext();

    return (
      <label
        ref={ref}
        className={cn(
          "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70",
          className,
        )}
        htmlFor={htmlFor ?? textField?.id}
        {...props}
      >
        {props.children}
        {textField?.required ? (
          <span className="ml-1 text-destructive" aria-hidden="true">
            *
          </span>
        ) : null}
      </label>
    );
  },
);
Label.displayName = "Label";

export { Label };
