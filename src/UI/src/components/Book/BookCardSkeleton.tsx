import { Skeleton } from "@heroui/react";

export default function BookCardSkeleton() {
    return (
        <div className="rounded border border-white bg-white p-2 shadow-md" aria-hidden="true">
            <Skeleton className="mb-2 aspect-[3/4] w-full rounded" />
            <Skeleton className="mb-2 h-6 w-3/4 rounded" />
            <Skeleton className="mb-3 h-4 w-1/2 rounded" />
            <div className="mb-1 flex justify-between">
                <Skeleton className="h-4 w-1/3 rounded" />
                <Skeleton className="h-4 w-1/2 rounded" />
            </div>
            <div className="flex justify-between">
                <Skeleton className="h-4 w-1/3 rounded" />
                <Skeleton className="h-4 w-1/4 rounded" />
            </div>
        </div>
    );
}
