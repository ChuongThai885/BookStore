import axios, { type AxiosRequestConfig, type AxiosInstance } from "axios";

export function createHttpClient(baseURL: string, options?: AxiosRequestConfig): AxiosInstance {
    const client = axios.create({
        baseURL,
        timeout: 10000,
        ...options,
    });

    client.interceptors.response.use(
        (response) => response,
        (error) => {
            if (error.response?.status === 401) {
                // Handle refresh token or redirect to Login
            }
            return Promise.reject(error);
        }
    );

    return client;
}
