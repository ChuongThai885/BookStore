import { createHttpClient } from "../../../utils/httpClient";
import { type Book, type TableData } from "../type";
import { API_PREFIX } from "../../../constants/apiPrefixes";

export const productClient = createHttpClient(API_PREFIX.PRODUCT);

export const getBooks = async (pageNumber: number, pageSize: number) => {
    const response = await productClient.post<TableData<Book>>("Book/get", {
        pageNumber,
        pageSize,
    });
    return response.data;
};
