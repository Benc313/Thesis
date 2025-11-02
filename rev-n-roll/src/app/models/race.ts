export enum RaceType {
  Drag = 0,
  Circuit = 1,
  Sprint = 2
}

export interface RaceUser {
  id: number;
  nickname: string;
}

export interface Race {
  id: number;
  name: string;
  description: string;
  creatorId: number;
  crewId?: number;
  raceType: RaceType;
  location: string;
  coordinates: string;
  date: string;
  private: boolean;
  users: RaceUser[];
}

export interface RaceRequest {
  name: string;
  description: string;
  crewId?: number;
  raceType: RaceType;
  location: string;
  coordinates: string;
  date: string;
  private: boolean;
}