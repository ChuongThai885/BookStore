import { useState } from "react";
import { BookCard } from "../../components/Book";
import { useQuery } from "@tanstack/react-query";
import { getBooks } from "../../features/Product/api/product.client";

export default function Product() {
    const pageSize = 10;
    const [currentPage, setCurrentPage] = useState<number>(1);

    const { data, isLoading, isError } = useQuery({
        queryKey: ["books", currentPage],
        queryFn: () => getBooks(currentPage, pageSize),
    });

    if (isLoading) return <div className="p-4 text-center">Loading...</div>;
    if (isError) return <div className="p-4 text-center text-red-500">An unexpected error occurred</div>;

    return (
        <div className="grid gap-4 p-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
            {data?.data?.map((book) => (
                <BookCard key={book.id} book={book} />
            ))}
        </div>
    );
}
