import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { loginApi, logoutApi } from "../api/auth.client";
import type { UserProfile } from "../type";
import { getCurrentUser } from "../api/auth.client";

export const AUTH_QUERY_KEY = ["auth", "currentUser"];

export const useAuth = () => {
    const queryClient = useQueryClient();

    const { data: currentUser, isLoading } = useQuery<UserProfile | null>({
        queryKey: AUTH_QUERY_KEY,
        queryFn: async () => {
            // Replace this with the actual API call to get the current user
            try {
                return await getCurrentUser();
            } catch {
                return null;
            }
        },
        staleTime: Infinity,
        retry: false,
    });

    const loginMutation = useMutation({
        mutationFn: loginApi,
        onSuccess: (data) => {
            queryClient.setQueryData(AUTH_QUERY_KEY, data);
        },
    });

    const logoutMutation = useMutation({
        mutationFn: logoutApi,
        onSuccess: () => {
            queryClient.setQueryData(AUTH_QUERY_KEY, null);
        },
    });

    return { currentUser, isLoading, login: loginMutation.mutateAsync, logout: logoutMutation.mutateAsync };
};
