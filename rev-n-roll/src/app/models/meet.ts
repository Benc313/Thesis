export enum MeetTags {
  CarsNCoffee = 0,
  Cruising = 1,
  MeetNGreet = 2,
  AmpsNWoofers = 3,
  Racing = 4,
  Tour = 5
}

export interface MeetUser {
  id: number;
  nickname: string;
}

export interface Meet {
  id: number;
  name: string;
  description: string;
  creatorId: number;
  crewId?: number;
  location: string;
  coordinates: string;
  date: string;
  private: boolean;
  tags: MeetTags[];
  users: MeetUser[];
}

export interface MeetRequest {
  name: string;
  description: string;
  crewId?: number;
  location: string;
  coordinates: string;
  date: string;
  private: boolean;
  tags: MeetTags[];
} 