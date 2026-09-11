import { Field, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

type UserSelectFieldProps = {
  id: string;
  label: string;
  name: string;
  options: string[];
  placeholder?: string;
  size?: "sm" | "default";
};

export function UserSelectField({
  id,
  label,
  name,
  options,
  placeholder = "Seleccione una opción",
  size = "default",
}: UserSelectFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Select name={name}>
        <SelectTrigger id={id} size={size} className="w-full">
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option} value={option}>
              {option}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </Field>
  );
}
