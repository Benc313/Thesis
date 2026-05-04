export enum Rank {
    Member = 0,
    Moderator = 1,
    Leader = 2,
}

export interface CrewMember {
    id: string;
    email: string;
    nickname: string;
    description?: string;
    imageLocation?: string;
    rank?: number; // 0=Member, 1=Moderator, 2=Leader
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
