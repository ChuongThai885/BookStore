export default function Login() {
    return (
        <div className="">
            <form>
                <input type="text" placeholder="Username" />
                <input type="password" placeholder="Password" />
                <button type="submit">Login</button>
                <span>Or login via external provider</span>
                <button type="button">Login with Google</button>
                <button type="button" disabled>
                    Login with Microsoft
                </button>
            </form>
        </div>
    );
}
