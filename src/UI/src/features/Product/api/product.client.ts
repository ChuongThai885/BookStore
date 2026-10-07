import { createHttpClient } from "../../../utils/httpClient";
import { type Book, type TableData } from "../type";

export const productClient = createHttpClient(import.meta.env.VITE_PRODUCT_API_URL);

export const getBooks = async (page: number, pageSize: number) => {
    const response = await productClient.post<TableData<Book>>("Book/get", {
        startIndex: (page - 1) * pageSize,
        pageSize,
    });
    console.log(response);
    return response.data;
};
