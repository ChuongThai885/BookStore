import { useState } from "react";
import { type Book } from "../../features/Product/type";

function getRandomInt(min: number, max: number): number {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

export const BookCard = ({ book }: { book: Book }) => {
    const [bookCoverImage] = useState(() => `/chiikawa${getRandomInt(1, 6)}.png`);

    return (
        <div className="rounded border border-white bg-white p-2 shadow-md">
            <img src={bookCoverImage} alt={book.title} className="mb-2 aspect-[3/4] w-full rounded object-cover" />
            <h2 className="text-lg font-medium">{book.title}</h2>
            <p className="text-sm text-gray-600">{book.genres.map((genre) => genre.name).join(", ")}</p>
            <div className="flex justify-between">
                <p className="text-sm">Author: </p>
                <p className="text-sm text-gray-600">{book.author?.name}</p>
            </div>
            <div className="flex justify-between">
                <p className="text-sm">Price: </p>
                <p className="text-sm text-gray-600">$ {book.price}</p>
            </div>
        </div>
    );
};
