"use client";

import { createContext, useContext } from "react";

export type TextFieldContextValue = {
  id: string;
  required: boolean;
};

const TextFieldContext = createContext<TextFieldContextValue | undefined>(
  undefined,
);

export const TextFieldProvider = TextFieldContext.Provider;

export function useTextFieldContext() {
  return useContext(TextFieldContext);
}

export function useTextFieldId() {
  return useTextFieldContext()?.id;
}
