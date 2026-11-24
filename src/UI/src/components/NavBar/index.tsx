import { NavLink } from "react-router-dom";
import { getAuthErrorMessage } from "../../features/Authentication/api/auth.client";
import { useAuth } from "../../features/Authentication/hooks/useAuth";

export default function NavBar() {
    const { currentUser, isAuthPending, authError, refetchAuth } = useAuth();

    return (
        <div className="relative z-10 flex justify-between bg-rose-200 p-2 shadow-md">
            <div className="flex gap-4">
                <NavLink to="/" className="mr-auto flex cursor-pointer items-center">
                    <img className="h-7 w-7" src="/favicon.svg" alt="Book Store Logo" />
                    <div className="p-2 font-medium">Rim dang yeu</div>
                </NavLink>
                <div className="p-2 font-medium">
                    <NavLink to="/products">Products</NavLink>
                </div>
                <div className="p-2 font-medium">
                    <NavLink to="/orders">Orders</NavLink>
                </div>
                <div className="p-2 font-medium">
                    <NavLink to="/about-us">About Us</NavLink>
                </div>
            </div>
            {isAuthPending ? (
                <span className="p-2" role="status">Checking your session...</span>
            ) : authError ? (
                <div className="p-2" role="alert">
                    Unable to check your session: {getAuthErrorMessage(authError)}{" "}
                    <button type="button" onClick={() => void refetchAuth()}>Try again</button>
                </div>
            ) : currentUser ? (
                <span className="p-2">Welcome back, {currentUser.displayName?.trim() || currentUser.email}</span>
            ) : (
                <NavLink to="/login" className="bg-white p-2">Login</NavLink>
            )}
        </div>
    );
}
