import { API_PREFIX } from "../../../constants/apiPrefixes";
import { createHttpClient } from "../../../utils/httpClient";
import type { LoginPayload, UserProfile } from "../type";

export const authClient = createHttpClient(API_PREFIX.AUTH);

export const getCurrentUser = async (): Promise<UserProfile> => {
    const response = await authClient.get("/Auth/me");
    return response.data;
};

export const loginApi = async (payload: LoginPayload): Promise<UserProfile> => {
    const response = await authClient.post<UserProfile>("/Auth/login", payload);
    return response.data;
};

export const logoutApi = async (): Promise<void> => {
    await authClient.post("/Auth/logout");
};
