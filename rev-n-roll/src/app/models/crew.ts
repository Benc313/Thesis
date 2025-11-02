import { User } from './user';


export interface Crew {
    id: number;
    name: string;
    description: string;
    imageLocation: string;
    users: User[]; // List of user members (UserResponse in C#)
}


export interface CrewRequest {
    name: string;
    description: string;
    imageLocation: string;
}