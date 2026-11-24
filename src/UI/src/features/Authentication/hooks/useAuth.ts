import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { isAxiosError } from "axios";
import { externalLoginApi, loginApi, logoutApi, refreshTokenApi, registerApi } from "../api/auth.client";
import type { UserProfile } from "../type";

export const AUTH_QUERY_KEY = ["auth", "currentUser"];

const restoreSession = async (): Promise<UserProfile | null> => {
    try {
        return await (navigator.locks
            ? navigator.locks.request("bookstore-refresh-token", () => refreshTokenApi())
            : refreshTokenApi());
    } catch (error) {
        if (isAxiosError(error) && error.response?.status === 401) return null;
        throw error;
    }
};

export const useAuth = () => {
    const queryClient = useQueryClient();

    const { data: currentUser, isPending: isAuthPending, error: authError, refetch: refetchAuth } = useQuery<UserProfile | null>({
        queryKey: AUTH_QUERY_KEY,
        queryFn: restoreSession,
        staleTime: Infinity,
        retry: false,
    });

    const onAuthenticated = (data: UserProfile) => {
        queryClient.setQueryData(AUTH_QUERY_KEY, data);
    };

    const loginMutation = useMutation({
        mutationFn: loginApi,
        onSuccess: onAuthenticated,
    });

    const registerMutation = useMutation({
        mutationFn: registerApi,
        onSuccess: onAuthenticated,
    });

    const externalLoginMutation = useMutation({
        mutationFn: externalLoginApi,
        onSuccess: onAuthenticated,
    });

    const refreshMutation = useMutation({
        mutationFn: refreshTokenApi,
        onSuccess: (data) => queryClient.setQueryData(AUTH_QUERY_KEY, data),
    });

    const logoutMutation = useMutation({
        mutationFn: logoutApi,
        onSuccess: () => {
            queryClient.setQueryData(AUTH_QUERY_KEY, null);
        },
    });

    return { currentUser, isAuthPending, authError, refetchAuth, loginMutation, registerMutation, externalLoginMutation, refreshMutation, logoutMutation };
};
