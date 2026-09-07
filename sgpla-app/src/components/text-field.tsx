"use client";

import { useId, type ComponentProps, type ReactNode } from "react";
import { Field } from "@/components/ui/field";
import { TextFieldProvider } from "./text-field-context";

type TextFieldProps = Omit<ComponentProps<typeof Field>, "children"> & {
  id?: string;
  required?: boolean;
  children: ReactNode;
};

export function TextField({
  id,
  required = false,
  children,
  ...props
}: TextFieldProps) {
  const generatedId = useId().replaceAll(":", "");

  return (
    <TextFieldProvider value={{ id: id ?? generatedId, required }}>
      <Field {...props}>{children}</Field>
    </TextFieldProvider>
  );
}
