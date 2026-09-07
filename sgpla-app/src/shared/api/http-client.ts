export type ValidationErrors = Record<string, string[]>;

type ProblemDetails = {
  detail?: string;
  title?: string;
  errors?: ValidationErrors;
};

export class HttpClientError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly validationErrors?: ValidationErrors,
  ) {
    super(message);
    this.name = "HttpClientError";
  }
}

type RequestOptions = Omit<RequestInit, "body" | "method"> & {
  body?: unknown;
};

function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === "object" && value !== null;
}

function getProblemMessage(payload: unknown, status: number) {
  if (!isProblemDetails(payload)) {
    return `La solicitud no pudo completarse (${status}).`;
  }

  return (
    payload.detail ??
    payload.title ??
    `La solicitud no pudo completarse (${status}).`
  );
}

async function request<T>(
  method: "GET" | "POST" | "PUT" | "DELETE",
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  const hasBody = options.body !== undefined;

  if (hasBody && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(path, {
    ...options,
    method,
    headers,
    credentials: "include",
    body: hasBody ? JSON.stringify(options.body) : undefined,
  });

  const isJson = response.headers
    .get("content-type")
    ?.toLowerCase()
    .includes("json");
  const payload: unknown = isJson ? await response.json() : undefined;

  if (!response.ok) {
    const problem = isProblemDetails(payload) ? payload : undefined;
    throw new HttpClientError(
      getProblemMessage(payload, response.status),
      response.status,
      problem?.errors,
    );
  }

  return payload as T;
}

export function withQuery(
  path: string,
  values: Record<string, string | number | undefined | null>,
) {
  const query = new URLSearchParams();

  for (const [key, value] of Object.entries(values)) {
    if (value !== undefined && value !== null && value !== "") {
      query.set(key, String(value));
    }
  }

  const queryString = query.toString();
  return queryString ? `${path}?${queryString}` : path;
}

export const httpClient = {
  get: <T>(path: string, options?: RequestOptions) =>
    request<T>("GET", path, options),
  post: <T>(path: string, body: unknown, options?: RequestOptions) =>
    request<T>("POST", path, { ...options, body }),
  put: <T>(path: string, body: unknown, options?: RequestOptions) =>
    request<T>("PUT", path, { ...options, body }),
  delete: (path: string, options?: RequestOptions) =>
    request<void>("DELETE", path, options),
};

export function getErrorMessage(error: unknown) {
  return error instanceof Error
    ? error.message
    : "Ocurrió un error inesperado. Intenta nuevamente.";
}
