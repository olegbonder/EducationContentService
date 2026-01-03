export type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
};

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export type ErrorType =
  | "validation"
  | "notFound"
  | "conflict"
  | "unauthorized"
  | "forbidden"
  | "serverError";

export class EnvelopeError extends Error {
  public readonly apiError: ApiError | undefined;
  public readonly type: ErrorType | undefined;

  constructor(apiError: ApiError) {
    const firstMessage = apiError.messages[0].message ?? "Неизвестная ошибка";

    super(firstMessage);
    this.apiError = apiError;
    this.type = apiError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get messages(): ErrorMessage[] {
    return this.apiError?.messages ?? [];
  }

  get firstMessage(): string {
    return this.apiError?.messages[0].message ?? "Неизвестная ошибка";
  }

  get allMessages(): string[] {
    return this.messages.map((message) => message.message);
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}
