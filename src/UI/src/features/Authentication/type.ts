export interface UserProfile {
    userId: string;
    email: string;
    displayName: string | null;
}

export interface LoginPayload {
    email: string;
    password: string;
}

export interface RegisterPayload extends LoginPayload {
    displayName?: string;
}

export interface ExternalLoginPayload {
    provider: "Google" | "Microsoft";
    token: string;
    confirmLink?: boolean;
    linkPassword?: string;
}

export interface AuthApiError {
    message?: string;
    detail?: string;
    title?: string;
    requireLinking?: boolean;
    email?: string;
    isExistingEmailConfirmed?: boolean;
    errors?: Record<string, string[]>;
}
