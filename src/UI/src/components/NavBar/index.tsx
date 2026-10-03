import { NavLink } from "react-router-dom";

export default function NavBar() {
  return (
    <div className="flex p-2 gap-4">
      <div className="bg-red-600 p-2">icon here</div>
      <div className="bg-red-600 p-2">
        <NavLink to="/products">Products</NavLink>
      </div>
      <div className="bg-red-600 p-2">
        <NavLink to="/orders">Orders</NavLink>
      </div>
      <div className="bg-white p-2">Login</div>
    </div>
  );
}
