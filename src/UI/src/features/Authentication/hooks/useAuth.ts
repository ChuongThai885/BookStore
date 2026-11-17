import { skipToken, useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { externalLoginApi, loginApi, logoutApi, refreshTokenApi, registerApi } from "../api/auth.client";
import type { UserProfile } from "../type";

export const AUTH_QUERY_KEY = ["auth", "currentUser"];

export const useAuth = () => {
    const queryClient = useQueryClient();

    const { data: currentUser } = useQuery<UserProfile | null>({
        queryKey: AUTH_QUERY_KEY,
        queryFn: skipToken,
        initialData: null,
        staleTime: Infinity,
    });

    const onAuthenticated = (data: UserProfile) => {
        queryClient.setQueryData(AUTH_QUERY_KEY, data);
        console.log("Login result:", data);
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

    return { currentUser, loginMutation, registerMutation, externalLoginMutation, refreshMutation, logoutMutation };
};
