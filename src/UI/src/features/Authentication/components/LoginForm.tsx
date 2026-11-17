import { Button } from "@heroui/react";
import { getAuthErrorMessage } from "../api/auth.client";
import { useAuth } from "../hooks/useAuth";

export default function LoginForm() {
    const { loginMutation } = useAuth();
    const inputClassName = "h-12 w-full rounded-xl border border-stone-300 bg-white px-4 text-stone-900 outline-none focus:border-rose-500 focus:ring-2 focus:ring-rose-200";

    return (
        <form
            className="space-y-5"
            onSubmit={(event) => {
                event.preventDefault();
                if (loginMutation.isPending) return;
                const data = new FormData(event.currentTarget);
                loginMutation.mutate({
                    email: String(data.get("email")).trim(),
                    password: String(data.get("password")),
                });
            }}
        >
            <div className="space-y-2">
                <label htmlFor="login-email" className="text-sm font-medium text-stone-800">Email</label>
                <input id="login-email" name="email" type="email" autoComplete="username" placeholder="Enter your email" maxLength={256} required className={inputClassName} />
            </div>
            <div className="space-y-2">
                <label htmlFor="login-password" className="text-sm font-medium text-stone-800">Password</label>
                <input id="login-password" name="password" type="password" autoComplete="current-password" placeholder="Enter your password" minLength={8} maxLength={128} required className={inputClassName} />
            </div>
            <Button type="submit" className="h-12 w-full bg-rose-700 text-white" isDisabled={loginMutation.isPending}>
                {loginMutation.isPending ? "Logging in..." : "Login"}
            </Button>
            {loginMutation.error && <p role="alert" className="text-sm text-red-700">{getAuthErrorMessage(loginMutation.error)}</p>}
            {loginMutation.isSuccess && <p role="status" className="text-sm text-green-800">Logged in successfully.</p>}
        </form>
    );
}
