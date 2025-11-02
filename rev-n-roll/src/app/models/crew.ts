import { User } from './user';

export enum Rank {
    Member = 0,
    Moderator = 1,
    Leader = 2,
}

export interface CrewMember extends User {

  rank: Rank; 
}


export interface Crew {
    id: number;
    name: string;
    description: string;
    imageLocation: string;
    users: CrewMember[]; 
}

export interface CrewRequest {
    name: string;
    description: string;
    imageLocation: string;
}

export interface UserCrewRequest {
  userId: number;
  rank: Rank;
}