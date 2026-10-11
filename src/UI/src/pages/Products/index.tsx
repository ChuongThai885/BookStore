import { useMemo, useState } from "react";
import { BookCard } from "../../components/Book";
import { useQuery } from "@tanstack/react-query";
import { getBooks } from "../../features/Product/api/product.client";
import { Pagination } from "@heroui/react";

export default function Product() {
    const pageSize = 10;
    const [currentPage, setCurrentPage] = useState<number>(1);

    const { data, isLoading, isError } = useQuery({
        queryKey: ["books", currentPage],
        queryFn: () => getBooks(currentPage, pageSize),
    });

    const pageItems: (number | "ellipsis")[] = useMemo(() => {
        const pages: (number | "ellipsis")[] = [];
        if (!data?.totalPages) return pages;
        if (data?.totalPages <= 7) {
            for (let i = 1; i <= data?.totalPages; i++) {
                pages.push(i);
            }
        } else {
            pages.push(1);
            if (currentPage > 3) {
                pages.push("ellipsis");
            }
            const start = Math.max(2, currentPage - 1);
            const end = Math.min(data?.totalPages - 1, currentPage + 1);
            for (let i = start; i <= end; i++) {
                pages.push(i);
            }
            if (currentPage < data?.totalPages - 2) {
                pages.push("ellipsis");
            }
            pages.push(data?.totalPages);
        }
        return pages;
    }, [data?.totalPages, currentPage]);

    if (isLoading) return <div className="p-4 text-center">Loading...</div>;
    if (isError) return <div className="p-4 text-center text-red-500">An unexpected error occurred</div>;

    return (
        <div className="flex h-full flex-col">
            <div className="flex-1">
                <div className="grid gap-4 p-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
                    {data?.data?.map((book) => (
                        <BookCard key={book.id} book={book} />
                    ))}
                </div>
            </div>
            <div className="p-4">
                <Pagination className="mt-auto">
                    <Pagination.Summary>
                        Showing {(currentPage - 1) * pageSize + 1}-{Math.min(currentPage * pageSize, data?.total || 0)} of {data?.total || 0} results
                    </Pagination.Summary>
                    <Pagination.Content>
                        <Pagination.Item>
                            <Pagination.Previous isDisabled={!data?.hasPreviousPage} onPress={() => setCurrentPage((p) => p - 1)}>
                                <Pagination.PreviousIcon />
                                <span>Previous</span>
                            </Pagination.Previous>
                        </Pagination.Item>
                        {pageItems.map((p, i) =>
                            p === "ellipsis" ? (
                                <Pagination.Item key={`ellipsis-${i}`}>
                                    <Pagination.Ellipsis />
                                </Pagination.Item>
                            ) : (
                                <Pagination.Item key={p}>
                                    <Pagination.Link isActive={p === currentPage} onPress={() => setCurrentPage(p)}>
                                        {p}
                                    </Pagination.Link>
                                </Pagination.Item>
                            )
                        )}
                        <Pagination.Item>
                            <Pagination.Next isDisabled={!data?.hasNextPage} onPress={() => setCurrentPage((p) => p + 1)}>
                                <span>Next</span>
                                <Pagination.NextIcon />
                            </Pagination.Next>
                        </Pagination.Item>
                    </Pagination.Content>
                </Pagination>
            </div>
        </div>
    );
}
