import { NavLink } from "react-router-dom";

export default function NavBar() {
    return (
        <div className="relative z-10 flex justify-between bg-white p-2 shadow-md">
            <div className="flex gap-4">
                <NavLink to="/" className="mr-auto flex cursor-pointer items-center">
                    <img className="h-7 w-7" src="/favicon.svg" alt="Book Store Logo" />
                    <div className="p-2">Rim dang yeu</div>
                </NavLink>
                <div className="mr-auto p-2">
                    <NavLink to="/products">Products</NavLink>
                </div>
                <div className="p-2">
                    <NavLink to="/orders">Orders</NavLink>
                </div>
            </div>
            <div className="bg-white p-2">Login</div>
        </div>
    );
}
