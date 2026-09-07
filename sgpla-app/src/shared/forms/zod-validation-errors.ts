import type { ZodError } from "zod";
import type { ValidationErrors } from "@/shared/api/http-client";

export function toZodValidationErrors(error: ZodError): ValidationErrors {
  return error.issues.reduce<ValidationErrors>((errors, issue) => {
    const field = issue.path[0]?.toString();

    if (!field) return errors;

    errors[field] ??= [];
    if (!errors[field].includes(issue.message)) {
      errors[field].push(issue.message);
    }

    return errors;
  }, {});
}
