"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  HttpClientError,
  getErrorMessage,
  type ValidationErrors,
} from "@/shared/api/http-client";
import {
  REGIONES,
  type EntidadAcademica,
  type GuardarEntidadAcademicaInput,
} from "../domain/entidad-academica";
import {
  useActualizarEntidadAcademica,
  useAreasAcademicas,
  useCrearEntidadAcademica,
  useEntidadAcademica,
} from "../presentation/entidades-academicas.queries";

type Props = {
  entidad?: EntidadAcademica;
  readOnly?: boolean;
};

type FormValues = Omit<GuardarEntidadAcademicaInput, "idAreaAcademica"> & {
  idAreaAcademica: string;
};

type TextFieldProps = {
  id: keyof GuardarEntidadAcademicaInput;
  label: string;
  value: string;
  onChange: (value: string) => void;
  maxLength?: number;
  readOnly?: boolean;
  required?: boolean;
  pattern?: string;
  inputMode?: "numeric" | "text";
  error?: string;
};

function TextField({
  id,
  label,
  value,
  onChange,
  maxLength,
  readOnly = false,
  required = false,
  pattern,
  inputMode,
  error,
}: TextFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Input
        id={id}
        name={id}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        maxLength={maxLength}
        disabled={readOnly}
        required={required}
        pattern={pattern}
        inputMode={inputMode}
        aria-invalid={Boolean(error)}
      />
      <FieldError>{error}</FieldError>
    </Field>
  );
}

type SelectFieldProps = {
  id: "idAreaAcademica" | "region";
  label: string;
  value?: string;
  options: Array<{ value: string; label: string }>;
  onChange: (value: string) => void;
  readOnly?: boolean;
  loading?: boolean;
  error?: string;
};

function SelectField({
  id,
  label,
  value,
  options,
  onChange,
  readOnly = false,
  loading = false,
  error,
}: SelectFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Select
        name={id}
        value={value}
        onValueChange={onChange}
        disabled={readOnly || loading}
      >
        <SelectTrigger id={id} className="w-full" aria-invalid={Boolean(error)}>
          <SelectValue
            placeholder={loading ? "Cargando opciones..." : `Seleccione ${label.toLowerCase()}`}
          />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <FieldError>{error}</FieldError>
    </Field>
  );
}

function toFormValues(entidad?: EntidadAcademica): FormValues {
  return {
    clave: entidad?.clave ?? "",
    nombre: entidad?.nombre ?? "",
    calleNumero: entidad?.calleNumero ?? "",
    colonia: entidad?.colonia ?? "",
    cp: entidad?.cp ?? "",
    municipio: entidad?.municipio ?? "",
    telefono: entidad?.telefono ?? "",
    extension: entidad?.extension ?? "",
    idAreaAcademica: entidad?.idAreaAcademica.toString() ?? "",
    region: entidad?.region ?? "",
  };
}

function toGuardarInput(values: FormValues): GuardarEntidadAcademicaInput {
  return {
    ...values,
    clave: values.clave.trim(),
    nombre: values.nombre.trim(),
    calleNumero: values.calleNumero.trim(),
    colonia: values.colonia.trim(),
    cp: values.cp.trim(),
    municipio: values.municipio.trim(),
    telefono: values.telefono.trim(),
    extension: values.extension.trim(),
    idAreaAcademica: Number(values.idAreaAcademica),
    region: values.region.trim(),
  };
}

export function EntidadFormView({ entidad, readOnly = false }: Props) {
  const router = useRouter();
  const areasQuery = useAreasAcademicas();
  const crearMutation = useCrearEntidadAcademica();
  const actualizarMutation = useActualizarEntidadAcademica();
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [formError, setFormError] = useState<string>();
  const [formValues, setFormValues] = useState<FormValues>(() =>
    toFormValues(entidad),
  );
  const isSaving = crearMutation.isPending || actualizarMutation.isPending;
  const areas = (areasQuery.data?.items ?? []).map((area) => ({
    value: area.idAreaAcademica.toString(),
    label: area.nombre,
  }));
  const regiones = REGIONES.map((region) => ({ value: region, label: region }));

  function updateField<K extends keyof FormValues>(
    field: K,
    value: FormValues[K],
  ) {
    setFormValues((current) => ({ ...current, [field]: value }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setValidationErrors({});
    setFormError(undefined);

    try {
      const input = toGuardarInput(formValues);

      if (entidad) {
        await actualizarMutation.mutateAsync({
          idEntidadAcademica: entidad.idEntidadAcademica,
          input,
        });
      } else {
        await crearMutation.mutateAsync(input);
      }

      router.push("/EntidadesAcademicas");
      router.refresh();
    } catch (error) {
      if (error instanceof HttpClientError) {
        setValidationErrors(error.validationErrors ?? {});
      }

      setFormError(getErrorMessage(error));
    }
  }

  const errorFor = (field: string) => validationErrors[field]?.[0];

  return (
    <form
      className="mx-auto max-w-5xl space-y-8 pt-6"
      onSubmit={readOnly ? undefined : handleSubmit}
    >
      <div className="grid gap-6">
        <div className="grid gap-6 sm:grid-cols-[minmax(8rem,1fr)_3fr]">
          <TextField
            id="clave"
            label="Clave"
            value={formValues.clave}
            onChange={(value) => updateField("clave", value)}
            maxLength={5}
            pattern="[0-9]{5}"
            inputMode="numeric"
            required
            readOnly={readOnly}
            error={errorFor("clave")}
          />
          <TextField
            id="nombre"
            label="Nombre"
            value={formValues.nombre}
            onChange={(value) => updateField("nombre", value)}
            maxLength={100}
            required
            readOnly={readOnly}
            error={errorFor("nombre")}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-[2fr_1fr_0.6fr_1fr]">
          <TextField
            id="calleNumero"
            label="Domicilio (Calle y Número)"
            value={formValues.calleNumero}
            onChange={(value) => updateField("calleNumero", value)}
            maxLength={150}
            required
            readOnly={readOnly}
            error={errorFor("calleNumero")}
          />
          <TextField
            id="colonia"
            label="Colonia"
            value={formValues.colonia}
            onChange={(value) => updateField("colonia", value)}
            maxLength={100}
            required
            readOnly={readOnly}
            error={errorFor("colonia")}
          />
          <TextField
            id="cp"
            label="C.P."
            value={formValues.cp}
            onChange={(value) => updateField("cp", value)}
            maxLength={5}
            required
            readOnly={readOnly}
            error={errorFor("cp")}
          />
          <TextField
            id="municipio"
            label="Municipio"
            value={formValues.municipio}
            onChange={(value) => updateField("municipio", value)}
            maxLength={100}
            required
            readOnly={readOnly}
            error={errorFor("municipio")}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <TextField
            id="telefono"
            label="Teléfono"
            value={formValues.telefono}
            onChange={(value) => updateField("telefono", value)}
            maxLength={30}
            required
            readOnly={readOnly}
            error={errorFor("telefono")}
          />
          <TextField
            id="extension"
            label="Extensión"
            value={formValues.extension}
            onChange={(value) => updateField("extension", value)}
            maxLength={5}
            required
            readOnly={readOnly}
            error={errorFor("extension")}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <SelectField
            id="idAreaAcademica"
            label="Área Académica"
            value={formValues.idAreaAcademica}
            options={areas}
            onChange={(value) => updateField("idAreaAcademica", value)}
            loading={areasQuery.isPending}
            readOnly={readOnly}
            error={errorFor("idAreaAcademica")}
          />
          <SelectField
            id="region"
            label="Región"
            value={formValues.region}
            options={regiones}
            onChange={(value) => updateField("region", value)}
            readOnly={readOnly}
            error={errorFor("region")}
          />
        </div>
      </div>

      {areasQuery.isError ? (
        <p className="text-sm font-medium text-destructive" role="alert">
          {getErrorMessage(areasQuery.error)}
        </p>
      ) : null}

      {formError ? (
        <p className="text-sm font-medium text-destructive" role="alert">
          {formError}
        </p>
      ) : null}

      <div className="flex justify-end gap-2">
        <Button
          nativeButton={false}
          render={<Link href="/EntidadesAcademicas" />}
          variant="outline"
        >
          {readOnly ? "Regresar" : "Cancelar"}
        </Button>
        {!readOnly ? (
          <Button type="submit" disabled={isSaving || areasQuery.isPending}>
            {isSaving ? "Guardando..." : "Guardar"}
          </Button>
        ) : null}
      </div>
    </form>
  );
}

export function EntidadFormLoader({
  idEntidadAcademica,
  readOnly = false,
}: {
  idEntidadAcademica: number | undefined;
  readOnly?: boolean;
}) {
  const entidadQuery = useEntidadAcademica(idEntidadAcademica);

  if (!idEntidadAcademica || idEntidadAcademica <= 0) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        El identificador de la entidad académica no es válido.
      </p>
    );
  }

  if (entidadQuery.isPending) {
    return <p className="pt-6 text-sm text-muted-foreground">Cargando entidad académica…</p>;
  }

  if (entidadQuery.isError || !entidadQuery.data) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        {entidadQuery.isError
          ? getErrorMessage(entidadQuery.error)
          : "No se encontró la entidad académica solicitada."}
      </p>
    );
  }

  return <EntidadFormView entidad={entidadQuery.data} readOnly={readOnly} />;
}
