import { FieldError } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { REGIONES } from "../../domain/entidad-academica";
import type { EntidadFormValues } from "../../application/entidad-form.model";
import { EntidadFormSelectField } from "./ui/entidad-form-select-field";

type EntidadFormFieldsProps = {
  values: EntidadFormValues;
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  readOnly: boolean;
  onChange: <K extends keyof EntidadFormValues>(
    field: K,
    value: EntidadFormValues[K],
  ) => void;
  errorFor: (field: string) => string | undefined;
};

export function EntidadFormFields({
  values,
  areas,
  areasLoading,
  readOnly,
  onChange,
  errorFor,
}: EntidadFormFieldsProps) {
  return (
    <>
      <FormSection
        title="Identidad"
        description="Información principal de la entidad académica."
      >
        <div className="flex flex-col gap-5">
          <TextField id="clave" required>
            <Label>Clave</Label>
            <Input
              name="clave"
              value={values.clave}
              onChange={(event) => onChange("clave", event.target.value)}
              maxLength={5}
              pattern="[0-9]{5}"
              inputMode="numeric"
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("clave"))}
            />
            <FieldError>{errorFor("clave")}</FieldError>
          </TextField>
          <TextField id="nombre" required>
            <Label>Nombre</Label>
            <Input
              name="nombre"
              value={values.nombre}
              onChange={(event) => onChange("nombre", event.target.value)}
              maxLength={100}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("nombre"))}
            />
            <FieldError>{errorFor("nombre")}</FieldError>
          </TextField>
        </div>
      </FormSection>

      <FormSection
        title="Clasificación"
        description="Área académica y región a la que pertenece."
      >
        <div className="flex flex-col gap-5">
          <EntidadFormSelectField
            id="idAreaAcademica"
            label="Área Académica"
            value={values.idAreaAcademica}
            options={areas}
            onChange={(value) => onChange("idAreaAcademica", value)}
            loading={areasLoading}
            readOnly={readOnly}
            error={errorFor("idAreaAcademica")}
          />
          <EntidadFormSelectField
            id="region"
            label="Región"
            value={values.region}
            options={REGIONES.map((region) => ({
              value: region,
              label: region,
            }))}
            onChange={(value) => onChange("region", value)}
            readOnly={readOnly}
            error={errorFor("region")}
          />
        </div>
      </FormSection>

      <FormSection
        title="Domicilio"
        description="Ubicación física de la entidad académica."
      >
        <div className="flex flex-col gap-5">
          <TextField id="calleNumero" required>
            <Label>Domicilio (Calle y Número)</Label>
            <Input
              name="calleNumero"
              value={values.calleNumero}
              onChange={(event) => onChange("calleNumero", event.target.value)}
              maxLength={150}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("calleNumero"))}
            />
            <FieldError>{errorFor("calleNumero")}</FieldError>
          </TextField>
          <div className="grid grid-cols-[3fr_1fr] gap-3">
            <TextField id="colonia" required>
              <Label>Colonia</Label>
              <Input
                name="colonia"
                value={values.colonia}
                onChange={(event) => onChange("colonia", event.target.value)}
                maxLength={100}
                disabled={readOnly}
                aria-invalid={Boolean(errorFor("colonia"))}
              />
              <FieldError>{errorFor("colonia")}</FieldError>
            </TextField>
            <TextField id="cp" required>
              <Label>C.P.</Label>
              <Input
                name="cp"
                value={values.cp}
                onChange={(event) => onChange("cp", event.target.value)}
                maxLength={5}
                disabled={readOnly}
                aria-invalid={Boolean(errorFor("cp"))}
              />
              <FieldError>{errorFor("cp")}</FieldError>
            </TextField>
          </div>
          <TextField id="municipio" required>
            <Label>Municipio</Label>
            <Input
              name="municipio"
              value={values.municipio}
              onChange={(event) => onChange("municipio", event.target.value)}
              maxLength={100}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("municipio"))}
            />
            <FieldError>{errorFor("municipio")}</FieldError>
          </TextField>
        </div>
      </FormSection>

      <FormSection
        title="Contacto"
        description="Datos para contactar a la entidad académica."
      >
        <div className="flex flex-col gap-5">
          <TextField id="telefono" required>
            <Label>Teléfono</Label>
            <Input
              name="telefono"
              value={values.telefono}
              onChange={(event) => onChange("telefono", event.target.value)}
              maxLength={30}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("telefono"))}
            />
            <FieldError>{errorFor("telefono")}</FieldError>
          </TextField>
          <TextField id="extension" required>
            <Label>Extensión</Label>
            <Input
              name="extension"
              value={values.extension}
              onChange={(event) => onChange("extension", event.target.value)}
              maxLength={5}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("extension"))}
            />
            <FieldError>{errorFor("extension")}</FieldError>
          </TextField>
        </div>
      </FormSection>
    </>
  );
}
