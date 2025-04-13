export interface User {
    id: string;
    email: string;
    nickname: string;
    description?: string;
    imageLocation?: string;
  }
  
  export interface UserRequest {
    email: string;
    nickname: string;
    description?: string;
    imageLocation?: string;
  }