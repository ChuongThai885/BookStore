export interface Genre {
    id: string;
    name: string;
}

export interface Author {
    id: string;
    name: string;
}

export interface Book {
    id: string;
    title: string;
    genres: Genre[];
    price: number;
    author: Author;
}

export interface TableData<T> {
    data: T[];
    total: number;
    currentPage: number;
    totalPages: number;
}
