import { Outlet } from "react-router-dom";
import NavBar from "../components/NavBar";

export default function Layout() {
    return (
        <div className="h-screen bg-red-100">
            <NavBar />
            <main className="bg-white">
                <Outlet />
            </main>
        </div>
    );
}
