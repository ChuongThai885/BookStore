import { isAxiosError } from "axios";
import { API_PREFIX } from "../../../constants/apiPrefixes";
import { createHttpClient } from "../../../utils/httpClient";
import type { AuthApiError, ExternalLoginPayload, LoginPayload, RegisterPayload, UserProfile } from "../type";

export const authClient = createHttpClient(`${import.meta.env.VITE_API_URL}${API_PREFIX.AUTH}`, {
    withCredentials: true,
});

export const loginApi = async (payload: LoginPayload): Promise<UserProfile> => {
    const response = await authClient.post<UserProfile>("login", payload);
    return response.data;
};

export const registerApi = async (payload: RegisterPayload): Promise<UserProfile> => {
    const response = await authClient.post<UserProfile>("register", payload);
    return response.data;
};

export const externalLoginApi = async (payload: ExternalLoginPayload): Promise<UserProfile> => {
    const response = await authClient.post<UserProfile>("external-login", payload);
    return response.data;
};

export const refreshTokenApi = async (): Promise<UserProfile> => {
    const response = await authClient.post<UserProfile>("refresh-token");
    return response.data;
};

export const logoutApi = async (): Promise<void> => {
    await authClient.post("logout");
};

export function getAuthErrorMessage(error: unknown): string {
    if (isAxiosError<AuthApiError>(error)) {
        const data = error.response?.data;
        if (error.response?.status === 409 && data?.requireLinking) {
            return data.message ?? "This email already has an account. Account linking is required.";
        }
        const validationErrors = data?.errors && Object.values(data.errors).flat().join(" ");
        return validationErrors || data?.detail || data?.message || data?.title || error.message;
    }
    return error instanceof Error ? error.message : "Authentication failed. Please try again.";
}
