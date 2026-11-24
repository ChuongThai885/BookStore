import { useState } from "react";
import { Button, Tooltip } from "@heroui/react";
import { GoogleLogin, GoogleOAuthProvider } from "@react-oauth/google";
import { getAuthErrorMessage } from "../api/auth.client";
import { useAuth } from "../hooks/useAuth";

export default function ExternalLogin() {
    const [googleScriptError, setGoogleScriptError] = useState(false);
    const [googleError, setGoogleError] = useState("");
    const { externalLoginMutation } = useAuth();
    const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim();

    return (
        <div className="space-y-3">
            {!googleClientId && (
                <p role="alert" className="text-sm text-red-700">
                    Google login is not configured. Set VITE_GOOGLE_CLIENT_ID.
                </p>
            )}
            {googleClientId && (
                <GoogleOAuthProvider clientId={googleClientId} onScriptLoadError={() => setGoogleScriptError(true)}>
                    <div className="flex justify-center" inert={externalLoginMutation.isPending}>
                        <GoogleLogin
                            ux_mode="popup"
                            theme="outline"
                            shape="pill"
                            size="large"
                            width={Math.min(384, window.innerWidth - 48)}
                            onSuccess={({ credential }) => {
                                setGoogleError("");
                                if (!credential) {
                                    setGoogleError("Google did not return an ID token. Please try again.");
                                    return;
                                }
                                console.log("Google credential:", credential);
                                externalLoginMutation.mutate({ provider: "Google", token: credential });
                            }}
                            onError={() => setGoogleError("Google login failed. Please try again.")}
                        />
                    </div>
                </GoogleOAuthProvider>
            )}
            {googleScriptError && (
                <p role="alert" className="text-sm text-red-700">
                    Unable to load Google login. Check your connection and reload the page.
                </p>
            )}
            {googleError && (
                <p role="alert" className="text-sm text-red-700">
                    {googleError}
                </p>
            )}
            {externalLoginMutation.isPending && (
                <p role="status" className="text-sm text-stone-600">
                    Completing Google login...
                </p>
            )}
            {externalLoginMutation.error && (
                <p role="alert" className="text-sm text-red-700">
                    {getAuthErrorMessage(externalLoginMutation.error)}
                </p>
            )}
            {externalLoginMutation.isSuccess && (
                <p role="status" className="text-sm text-green-800">
                    Logged in successfully.
                </p>
            )}
            <Tooltip delay={0}>
                <Tooltip.Trigger className="w-full rounded-xl" tabIndex={0} aria-label="Microsoft login unavailable">
                    <Button type="button" className="h-12 w-full bg-rose-700 text-white disabled:opacity-100" isDisabled>
                        Login with Microsoft
                    </Button>
                </Tooltip.Trigger>
                <Tooltip.Content arrowBoundaryOffset={0}>this provider is not available at this time</Tooltip.Content>
            </Tooltip>
        </div>
    );
}
