import { createBrowserRouter, Navigate } from "react-router-dom";
import Orders from "../pages/Orders";
import Product from "../pages/Products";
import Layout from "../layouts";
import AboutUs from "../pages/AboutUs";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <Layout />,
        children: [
            {
                index: true,
                element: <Navigate to="/products" replace />,
            },
            {
                path: "products",
                element: <Product />,
            },
            {
                path: "orders",
                element: <Orders />,
            },
            {
                path: "about-us",
                element: <AboutUs />,
            },
        ],
    },
]);
