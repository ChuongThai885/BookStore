import ExternalLogin from "../../features/Authentication/components/ExternalLogin";
import LoginForm from "../../features/Authentication/components/LoginForm";

export default function Login() {
    return (
        <section className="grid min-h-screen bg-[#fffaf4] lg:grid-cols-2" aria-labelledby="login-title">
            <div className="relative min-h-64 lg:min-h-screen">
                <img src="/background.jpg" alt="A cozy illustrated bookstore" className="absolute inset-0 h-full w-full object-cover object-center" />
            </div>
            <div className="flex items-center justify-center px-6 py-12 sm:px-12 lg:py-16">
                <div className="w-full max-w-sm">
                    <p className="mb-3 text-xs font-semibold tracking-[0.25em] text-rose-700 uppercase">Your next chapter</p>
                    <h1 id="login-title" className="text-4xl font-semibold tracking-tight text-stone-900">Welcome back</h1>
                    <p className="mt-3 mb-8 text-stone-600">Log in and find your next favorite book.</p>
                    <LoginForm />
                    <div className="my-7 flex items-center gap-4 text-xs text-stone-500">
                        <span className="h-px flex-1 bg-stone-200" />
                        <span>Or login via external provider</span>
                        <span className="h-px flex-1 bg-stone-200" />
                    </div>
                    <ExternalLogin />
                </div>
            </div>
        </section>
    );
}
