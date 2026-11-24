import { Navigate, Outlet } from "react-router-dom";
import { getAuthErrorMessage } from "../../features/Authentication/api/auth.client";
import { useAuth } from "../../features/Authentication/hooks/useAuth";

export default function GuestRoute() {
    const { currentUser, isAuthPending, authError, refetchAuth } = useAuth();

    if (isAuthPending) return <p role="status">Checking your session...</p>;
    if (authError) {
        return (
            <div role="alert">
                <p>Unable to check your session: {getAuthErrorMessage(authError)}</p>
                <button type="button" onClick={() => void refetchAuth()}>Try again</button>
            </div>
        );
    }
    return currentUser ? <Navigate to="/" replace /> : <Outlet />;
}