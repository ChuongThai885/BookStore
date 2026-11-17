export interface UserProfile {
    id: string;
    username: string;
    email: string;
}

export interface LoginPayload {
    username: string;
    password: string;
}
