import { Outlet } from "react-router-dom";
import NavBar from "../components/NavBar";

export default function Layout() {
    return (
        <div className="flex h-screen flex-col">
            <NavBar />
            <main className="flex-1 overflow-y-auto bg-rose-100">
                <Outlet />
            </main>
            <footer className="bg-rose-200 p-2 text-center shadow-md">&copy; 2026 BookStore - Rim dang yeu</footer>
        </div>
    );
}
